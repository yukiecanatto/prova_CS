using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebSocketServer.Producers
{
    public class RandomProducer : BackgroundService
    {
        private readonly ILogger<RandomProducer> _logger;
        private readonly TopicConfiguration _config;

        public RandomProducer(ILogger<RandomProducer> logger, TopicConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                string message = string.Format(_config.MessageFormat, RandomEventGenerator.GenerateEvent());
                await BroadcastService.BroadcastMessage(message, _config.Name);
                _logger.LogInformation($"Random Producer: {message}");
                await Task.Delay(_config.Interval, stoppingToken);
            }
        }
    }
}
