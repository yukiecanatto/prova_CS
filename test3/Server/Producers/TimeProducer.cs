using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebSocketServer.Producers
{
    public class TimeProducer : BackgroundService
    {
        private readonly ILogger<TimeProducer> _logger;
        private readonly TopicConfiguration _config;

        public TimeProducer(ILogger<TimeProducer> logger, TopicConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                string message = string.Format(_config.MessageFormat, DateTime.UtcNow);
                await BroadcastService.BroadcastMessage(message, _config.Name);
                _logger.LogInformation($"Time Producer: {message}");
                await Task.Delay(_config.Interval, stoppingToken);
            }
        }
    }
}
