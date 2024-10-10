using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using WebSocketServer.Producers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]

namespace WebSocketServer
{
    public class Program
    {
        private static readonly ClientManager _clientManager = new ClientManager();
        private static ServiceManager _serviceManager;
        private static ILogger<Program>? _logger;
        private static IConfiguration _configuration = null!;

        public static async Task Main(string[] args)
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                .Build();

            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsoleFormatter<CustomConsoleFormatter, ConsoleFormatterOptions>();
                builder.AddConsole(options =>
                {
                    options.FormatterName = "customFormatter";
                });
            });

            _logger = loggerFactory.CreateLogger<Program>();

            ServerStateManager.InitializeUserData();

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                _logger?.LogInformation("Server shutting down...");
                _clientManager.AbortAllClients();
                ServerStateManager.SaveServerState();
                Environment.Exit(0);
            };

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton(_configuration);
                    services.AddSingleton<ServiceManager>();

                    // Manually register each producer
                    RegisterProducers(services);
                })
                .Build();

            _serviceManager = host.Services.GetRequiredService<ServiceManager>();

            await host.StartAsync();

            _serviceManager.StartAllServices();

            var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:5000/");
            listener.Start();

            _logger?.LogInformation("Server started at http://localhost:5000/");

            while (true)
            {
                var context = await listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    var wsContext = await context.AcceptWebSocketAsync(null);
                    _clientManager.HandleNewConnection(wsContext.WebSocket);
                }
                else if (context.Request.HttpMethod == "POST" && context.Request.Url.AbsolutePath == "/configure")
                {
                    await HandleConfigureRequest(context);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }

        private static async Task HandleConfigureRequest(HttpListenerContext context)
        {
            try
            {
                using var reader = new StreamReader(context.Request.InputStream);
                var requestBody = await reader.ReadToEndAsync();
                Console.WriteLine($"Received configuration request: {requestBody}");

                var config = JsonSerializer.Deserialize<TopicConfiguration>(requestBody);

                if (config != null)
                {
                    // Update your configuration here
                    Console.WriteLine($"Updated configuration for topic {config.Name}: Interval = {config.Interval}");
                    context.Response.StatusCode = 200;
                    await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Configuration updated successfully"));
                }
                else
                {
                    context.Response.StatusCode = 400;
                    await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Invalid configuration data"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling configure request: {ex}");
                context.Response.StatusCode = 500;
                await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes($"Internal server error: {ex.Message}"));
            }
            finally
            {
                context.Response.Close();
            }
        }

        private static void RegisterProducers(IServiceCollection services)
        {
            var topicConfigurations = _configuration.GetSection("TopicConfigurations").Get<List<TopicConfiguration>>();
            if (topicConfigurations == null)
            {
                throw new InvalidOperationException("Topic configurations are not properly configured in appSettings.json");
            }

            foreach (var config in topicConfigurations)
            {
                services.AddSingleton(config);

                switch (config.Name)
                {
                    case "Cpu":
                        services.AddSingleton<IHostedService>(sp =>
                            new CpuProducer(sp.GetRequiredService<ILogger<CpuProducer>>(), config));
                        break;

                    case "Time":
                        services.AddSingleton<IHostedService>(sp =>
                            new TimeProducer(sp.GetRequiredService<ILogger<TimeProducer>>(), config));
                        break;

                    case "Weather":
                        services.AddSingleton<IHostedService>(sp =>
                            new WeatherProducer(sp.GetRequiredService<ILogger<WeatherProducer>>(), config));
                        break;

                    case "News":
                        services.AddSingleton<IHostedService>(sp =>
                            new NewsProducer(sp.GetRequiredService<ILogger<NewsProducer>>(), config));
                        break;

                    case "Sports":
                        services.AddSingleton<IHostedService>(sp =>
                            new SportsProducer(sp.GetRequiredService<ILogger<SportsProducer>>(), config));
                        break;

                    case "Random":
                        services.AddSingleton<IHostedService>(sp =>
                            new RandomProducer(sp.GetRequiredService<ILogger<RandomProducer>>(), config));
                        break;

                    case "Jokes":
                        services.AddSingleton<IHostedService>(sp =>
                            new JokesProducer(sp.GetRequiredService<ILogger<JokesProducer>>(), config));
                        break;

                    default:
                        throw new ArgumentException($"Unknown producer type: {config.Name}");
                }
            }
        }
    }

    public class ClientManager
    {
        private static readonly ConcurrentDictionary<WebSocket, ConcurrentBag<string>> _clientSubscriptions = new ConcurrentDictionary<WebSocket, ConcurrentBag<string>>();

        public static bool IsClientSubscribedToTopic(WebSocket client, string topic)
        {
            if (_clientSubscriptions.TryGetValue(client, out var subscriptions))
            {
                return subscriptions.Contains(topic);
            }
            return false;
        }

        public void HandleNewConnection(WebSocket webSocket)
        {
            _clientSubscriptions.TryAdd(webSocket, new ConcurrentBag<string>());
            Guid userId = ServerStateManager.RegisterNewUser();

            Task.Run(async () =>
            {
                var buffer = new byte[1024 * 4];
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                while (!result.CloseStatus.HasValue)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    LoggerHelper.LogReceivedMessage(message);

                    if (message.StartsWith("SUBSCRIBE:"))
                    {
                        var topic = message.Split(':')[1].Trim();
                        if (!string.IsNullOrEmpty(topic))
                        {
                            _clientSubscriptions[webSocket].Add(topic);
                            LoggerHelper.LogSubscription(topic);
                        }
                    }

                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }

                _clientSubscriptions.TryRemove(webSocket, out _);
                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                LoggerHelper.LogConnectionClosed();
            });
        }

        public void AbortAllClients()
        {
            foreach (var client in _clientSubscriptions.Keys)
            {
                client.Abort();
            }
        }

        public static IEnumerable<WebSocket> GetClients()
        {
            return _clientSubscriptions.Keys;
        }
    }

    public class ServiceManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ServiceManager> _logger;

        public ServiceManager(IServiceProvider serviceProvider, ILogger<ServiceManager> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public void StartAllServices()
        {
            var services = _serviceProvider.GetServices<IHostedService>();
            foreach (var service in services)
            {
                service.StartAsync(CancellationToken.None);
                _logger.LogInformation($"Started service: {service.GetType().Name}");
            }
        }
    }

    public class TopicConfiguration
    {
        public string Name { get; set; } = string.Empty;
        public int Interval { get; set; }
        public string MessageFormat { get; set; } = string.Empty;
    }

    public static class ServerStateManager
    {
        private static readonly Dictionary<Guid, string> _userData = new Dictionary<Guid, string>();

        public static void InitializeUserData()
        {
            for (int i = 0; i < 10; i++)
            {
                var userId = Guid.NewGuid();
                _userData[userId] = $"User{i}";
                LoggerHelper.LogReceivedMessage($"Initialized data for user {userId}");
            }
        }

        public static Guid RegisterNewUser()
        {
            var newUser = Guid.NewGuid();
            _userData[newUser] = $"User{newUser.ToString().Substring(0, 4)}";
            LoggerHelper.LogReceivedMessage($"New user registered: {newUser}");
            return newUser;
        }

        public static void SaveServerState()
        {
            Console.WriteLine("[LOG] Saving server state...");
            foreach (var entry in _userData)
            {
                Console.WriteLine($"[LOG] User: {entry.Key}, Name: {entry.Value}");
            }
        }
    }

    public static class LoggerHelper
    {
        public static void LogReceivedMessage(string message)
        {
            Console.WriteLine($"[LOG] Received: {message}");
        }

        public static void LogSubscription(string topic)
        {
            Console.WriteLine($"[LOG] Client subscribed to topic '{topic}'");
        }

        public static void LogConnectionClosed()
        {
            Console.WriteLine("[LOG] Connection closed.");
        }
    }

    public static class BroadcastService
    {
        public static async Task BroadcastMessage(string message, string topic)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            var tasks = new List<Task>();

            Console.WriteLine($">>> Broadcasting topic '{topic}'");
            foreach (var client in ClientManager.GetClients())
            {
                if (client.State == WebSocketState.Open && ClientManager.IsClientSubscribedToTopic(client, topic))
                {
                    tasks.Add(client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None));
                }
            }

            await Task.WhenAll(tasks);
        }
    }
}

public class CustomConsoleFormatter : ConsoleFormatter
{
    public CustomConsoleFormatter() : base("customFormatter") { }

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        var originalColor = Console.ForegroundColor;

        ConsoleColor logLevelColor = originalColor;
        string logLevelText = logEntry.LogLevel.ToString();
        switch (logEntry.LogLevel)
        {
            case LogLevel.Information:
                logLevelColor = ConsoleColor.Green;
                logLevelText = "Info";
                break;
            case LogLevel.Warning:
                logLevelColor = ConsoleColor.Yellow;
                logLevelText = "Warn";
                break;
            case LogLevel.Error:
            case LogLevel.Critical:
                logLevelColor = ConsoleColor.Red;
                break;
            case LogLevel.Debug:
                logLevelColor = ConsoleColor.Blue;
                break;
            case LogLevel.Trace:
                logLevelColor = ConsoleColor.Cyan;
                break;
            default:
                break;
        }

        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);

        if (!string.IsNullOrEmpty(message))
        {
            Console.Out.Write($"{timestamp} ");
            Console.ForegroundColor = logLevelColor;
            Console.Out.Write($"{logLevelText}: ");
            Console.ForegroundColor = originalColor;
            Console.Out.WriteLine($"{message}");
        }

        Console.ForegroundColor = originalColor;
    }
}