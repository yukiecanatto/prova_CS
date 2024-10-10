using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebSocketServer.Producers
{
    public class CpuProducer : BackgroundService
    {
        private readonly ILogger<CpuProducer> _logger;
        private readonly TopicConfiguration _config;

        public CpuProducer(ILogger<CpuProducer> logger, TopicConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var cpuUsage = new PerformanceCounter("Processor", "% Processor Time", "_Total");

            while (!stoppingToken.IsCancellationRequested)
            {
                cpuUsage.NextValue();
                float usage = cpuUsage.NextValue();
                string message = string.Format(_config.MessageFormat, usage);
                await BroadcastService.BroadcastMessage(message, _config.Name);
                _logger.LogInformation($"CPU Producer: {message}");
                await Task.Delay(_config.Interval, stoppingToken);
            }
        }
    }
}
