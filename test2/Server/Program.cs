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

[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]

namespace WebSocketServer
{
    // Main entry point
    public class Program
    {
        private static readonly ClientManager _clientManager = new ClientManager();
        private static readonly ServiceManager _serviceManager = new ServiceManager();
        private static ILogger<Program>? _logger;

        public static async Task Main(string[] args)
        {
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

            var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:5000/");
            listener.Start();

            _logger?.LogInformation("Server started at ws://localhost:5000/");

            _serviceManager.StartAllServices(); // Centralized service management

            while (true)
            {
                var context = await listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    var wsContext = await context.AcceptWebSocketAsync(null);
                    _clientManager.HandleNewConnection(wsContext.WebSocket);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }
    }

    // Class for managing clients
    public class ClientManager
    {
        private static readonly ConcurrentDictionary<WebSocket, ConcurrentBag<string>> _clientSubscriptions = new ConcurrentDictionary<WebSocket, ConcurrentBag<string>>();


    // Implement the IsClientSubscribedToTopic method
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

    // Centralized service manager
    public class ServiceManager
    {
        private readonly List<IService> _services;

        public ServiceManager()
        {
            _services = new List<IService>
            {
                new TimeService(),
                new CpuService(),
                // new NewsService(),
                // new WeatherService(),
                // new SportsService(),
                // new RandomEventService()
            };
        }

        public void StartAllServices()
        {
            foreach (var service in _services)
            {
                service.Start();
            }
        }
    }

    // Base service interface
    public interface IService
    {
        void Start();
    }

    // Service class for time updates
    public class TimeService : IService
    {
        public async void Start()
        {
            while (true)
            {
                await Task.Delay(7000);
                DateTime utcDateTime = DateTime.UtcNow;
                string iso8601Formated = utcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                string message = $"TIME: {iso8601Formated}";
                
                await BroadcastService.BroadcastMessage(message, "TIME");
                LoggerHelper.LogReceivedMessage(message);
                await Task.Delay(3000);
            }
        }
    }

    // Service class for CPU monitoring
    public class CpuService : IService
    {
        public async void Start()
        {
            var cpuUsage = new PerformanceCounter("Processor", "% Processor Time", "_Total");

            while (true)
            {   
                cpuUsage.NextValue();
                await Task.Delay(5000);
                float usage = cpuUsage.NextValue();
                string message = $"CPU: {usage}%";
                await BroadcastService.BroadcastMessage(message, "CPU");
                LoggerHelper.LogReceivedMessage(message);
                
            }
        }
    }

    // Service class for news updates
    public class NewsService : IService
    {
        public async void Start()
        {
            while (true)
            {
                string message = $"NEWS UPDATE: {TopicGenerator.GetRandomTopic()} - Latest update...";
                await BroadcastService.BroadcastMessage(message, "NEWS");
                LoggerHelper.LogReceivedMessage(message);
                await Task.Delay(10000);
            }
        }
    }

    // Service class for weather updates
    public class WeatherService : IService
    {
        public async void Start()
        {
            while (true)
            {
                string message = $"WEATHER: {WeatherDataProvider.GetWeather()}";
                await BroadcastService.BroadcastMessage(message, "WEATHER");
                LoggerHelper.LogReceivedMessage(message);
                await Task.Delay(13000);
            }
        }
    }

    // Service class for sports updates
    public class SportsService : IService
    {
        public async void Start()
        {
            while (true)
            {
                string message = $"SPORTS UPDATE: {SportsDataProvider.GetLatestScores()}";
                await BroadcastService.BroadcastMessage(message, "SPORTS");
                LoggerHelper.LogReceivedMessage(message);
                await Task.Delay(16000);
            }
        }
    }

    // Random event service to simulate random events
    public class RandomEventService : IService
    {
        public async void Start()
        {
            while (true)
            {
                string message = $"RANDOM EVENT: {RandomEventGenerator.GenerateEvent()}";
                await BroadcastService.BroadcastMessage(message, "RANDOM");
                LoggerHelper.LogReceivedMessage(message);
                await Task.Delay(19000);
            }
        }
    }

    // Class for managing server state and user data
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

    // Helper class for logging
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

    // Utility class for broadcasting messages
    public static class BroadcastService
    {
        public static async Task BroadcastMessage(string message, string topic = null)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            var tasks = new List<Task>();

            Console.WriteLine($">>> Broadcasting topic '{topic}'");
            foreach (var client in ClientManager.GetClients())
            {
            if (client.State == WebSocketState.Open)
                {
                    tasks.Add(client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None));
                }
            }

            await Task.WhenAll(tasks);
        }
    }

    // Utility class for generating topics
    public static class TopicGenerator
    {
        private static readonly List<string> _availableTopics = new List<string> { "TIME", "CPU", "NEWS", "SPORTS", "WEATHER" };

        public static string GetRandomTopic()
        {
            Random rnd = new Random();
            return _availableTopics[rnd.Next(_availableTopics.Count)];
        }
    }

    // Data provider for weather information
    public static class WeatherDataProvider
    {
        public static string GetWeather()
        {
            // Simulate getting weather data
            return "Sunny, 25°C";
        }
    }

    // Data provider for sports scores
    public static class SportsDataProvider
    {
        public static string GetLatestScores()
        {
            // Simulate getting sports scores
            return "Team A 3 - 2 Team B";
        }
    }

    // Random event generator
    public static class RandomEventGenerator
    {
        private static readonly List<string> _events = new List<string> { "Event A", "Event B", "Event C" };

        public static string GenerateEvent()
        {
            Random rnd = new Random();
            return _events[rnd.Next(_events.Count)];
        }
    }
}

// Custom Console Formatter class
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