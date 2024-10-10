using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Moq;
using WebSocketServer.Producers;
using WebSocketServer;  
using Xunit;

namespace ServerTests
{
    [Collection("Unit Tests")]
    public class UnitTests
    {
        [Fact]
        public async Task RandomProducer_ExecuteAsync_CallsLogger()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RandomProducer>>();
            var config = new WebSocketServer.TopicConfiguration { Name = "RANDOM", MessageFormat = "RANDOM: {0}", Interval = 100 };
            var producer = new RandomProducer(loggerMock.Object, config);

            // Act
            await RunProducerForOneIteration(producer);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Random Producer:")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce());
        }

        [Fact]
        public async Task CpuProducer_ExecuteAsync_CallsLogger()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CpuProducer>>();
            var config = new WebSocketServer.TopicConfiguration { Name = "CPU", MessageFormat = "CPU: {0}%", Interval = 100 };
            var producer = new CpuProducer(loggerMock.Object, config);

            // Act
            await RunProducerForOneIteration(producer);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CPU Producer:")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce());
        }

        [Fact]
        public async Task SportsProducer_ExecuteAsync_CallsLogger()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SportsProducer>>();
            var config = new TopicConfiguration { Name = "SPORTS", MessageFormat = "SPORTS: {0}", Interval = 100 };
            var producer = new SportsProducer(loggerMock.Object, config);

            // Act
            await RunProducerForOneIteration(producer);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Sports Producer:")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce());
        }

        [Fact]
        public async Task WeatherProducer_ExecuteAsync_CallsLogger()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<WeatherProducer>>();
            var config = new TopicConfiguration { Name = "WEATHER", MessageFormat = "WEATHER: {0}", Interval = 100 };
            var producer = new WeatherProducer(loggerMock.Object, config);

            // Act
            await RunProducerForOneIteration(producer);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Weather Producer:")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce());
        }

        [Fact]
        public async Task NewsProducer_ExecuteAsync_CallsLogger()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NewsProducer>>();
            var config = new TopicConfiguration { Name = "NEWS", MessageFormat = "NEWS: {0}", Interval = 100 };
            var producer = new NewsProducer(loggerMock.Object, config);

            // Act
            await RunProducerForOneIteration(producer);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("News Producer:")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce());
        }

        [Fact]
        public async Task TimeProducer_ExecuteAsync_CallsLogger()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TimeProducer>>();
            var config = new TopicConfiguration { Name = "TIME", MessageFormat = "TIME: {0}", Interval = 100 };
            var producer = new TimeProducer(loggerMock.Object, config);

            // Act
            await RunProducerForOneIteration(producer);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Time Producer:")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce());
        }

        private async Task RunProducerForOneIteration(IHostedService producer)
        {
            var cts = new CancellationTokenSource();
            var task = producer.StartAsync(cts.Token);
            await Task.Delay(50); // Reduced delay time
            cts.Cancel();
            await task;
        }

        [Fact]
        public async Task JokesProducer_ExecuteAsync_CallsLogger()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<JokesProducer>>();
            var config = new TopicConfiguration { Name = "JOKES", MessageFormat = "Here's your JOKE: {0}", Interval = 100 };
            var producer = new JokesProducer(loggerMock.Object, config);

            // Act
            await RunProducerForOneIteration(producer);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Jokes Producer:")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce());
        }
    }
}
