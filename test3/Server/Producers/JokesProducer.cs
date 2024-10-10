using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebSocketServer.Producers
{
    public class JokesProducer : BackgroundService
    {
        private readonly ILogger<JokesProducer> _logger;
        private readonly TopicConfiguration _config;

        public JokesProducer(ILogger<JokesProducer> logger, TopicConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                string message = string.Format(_config.MessageFormat, JokesGenerator.GetRandomJokes());
                await BroadcastService.BroadcastMessage(message, _config.Name);
                _logger.LogInformation($"Jokes Producer: {message}");
                await Task.Delay(_config.Interval, stoppingToken);
            }
        }
    }
}
