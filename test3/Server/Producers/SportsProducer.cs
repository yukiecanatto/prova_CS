using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebSocketServer.Producers
{
    public class SportsProducer : BackgroundService
    {
        private readonly ILogger<SportsProducer> _logger;
        private readonly TopicConfiguration _config;

        public SportsProducer(ILogger<SportsProducer> logger, TopicConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                string message = string.Format(_config.MessageFormat, SportsDataProvider.GetLatestScores());
                await BroadcastService.BroadcastMessage(message, _config.Name);
                _logger.LogInformation($"Sports Producer: {message}");
                await Task.Delay(_config.Interval, stoppingToken);
            }
        }
    }
}
