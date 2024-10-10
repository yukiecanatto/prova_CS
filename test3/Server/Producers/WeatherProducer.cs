using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebSocketServer.Producers
{
    public class WeatherProducer : BackgroundService
    {
        private readonly ILogger<WeatherProducer> _logger;
        private readonly TopicConfiguration _config;

        public WeatherProducer(ILogger<WeatherProducer> logger, TopicConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                string message = string.Format(_config.MessageFormat, WeatherDataProvider.GetWeather());
                await BroadcastService.BroadcastMessage(message, _config.Name);
                _logger.LogInformation($"Weather Producer: {message}");
                await Task.Delay(_config.Interval, stoppingToken);
            }
        }
    }
}
