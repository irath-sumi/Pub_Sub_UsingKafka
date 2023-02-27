using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConsumerApplication.Service.Tests
{

    [TestClass]
    public class KafkaConsumerServiceTests
    {
        /// <summary>
        /// Test that the StartAsync method subscribes to the correct Kafka topic
        /// </summary>
        /// <returns></returns>

        [TestMethod]       
        public async Task KafkaConsumerService_StartAsync_SubscribesToCorrectTopic()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    {"Kafka:BootstrapServers", "localhost:9092"},
                    {"Kafka:Consumer:GroupId", "test-group"},
                    {"Kafka:MaxRetryAttempts", "3"},
                    {"Kafka:DlqTopic", "test-dlq"},
                    {"Kafka:Consumer:Topic", "test-topic"}
                })
                .Build();
            var loggerFactory = new LoggerFactory();
            var consumerService = new KafkaConsumerService(configuration, loggerFactory);
            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            await consumerService.StartAsync(cancellationTokenSource.Token);

            // Assert
            Assert.AreEqual("test-topic", consumerService._consumer.Subscription.First());
        }

        /// <summary>
        /// Test that the StartAsync method starts the RunAsync method
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task KafkaConsumerService_StartAsync_StartsRunAsync()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                    {
                        {"Kafka:BootstrapServers", "localhost:9092"},
                        {"Kafka:Consumer:GroupId", "test-group"},
                        {"Kafka:MaxRetryAttempts", "3"},
                        {"Kafka:DlqTopic", "test-dlq"},
                        {"Kafka:Consumer:Topic", "test-topic"}
                    })
            .Build();
            var loggerFactory = new LoggerFactory();
            var consumerService = new KafkaConsumerService(configuration, loggerFactory);
            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            await consumerService.StartAsync(cancellationTokenSource.Token);

            // Assert
            Assert.IsFalse(consumerService._runningTask.IsCompleted);

        }

        /// <summary>
        /// Test that the `StopAsync` method cancels the `RunAsync` method
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task KafkaConsumerService_StopAsync_CancelsRunAsync()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                {"Kafka:BootstrapServers", "localhost:9092"},
                {"Kafka:Consumer:GroupId", "test-group"},
                {"Kafka:MaxRetryAttempts", "3"},
                {"Kafka:DlqTopic", "test-dlq"},
                {"Kafka:Consumer:Topic", "test-topic"}
            })
            .Build();
            var loggerFactory = new LoggerFactory();
            var consumerService = new KafkaConsumerService(configuration, loggerFactory);
            var cancellationTokenSource = new CancellationTokenSource();
            await consumerService.StartAsync(cancellationTokenSource.Token);

            // Act
            await consumerService.StopAsync(cancellationTokenSource.Token);

            // Assert
            Assert.IsTrue(consumerService!._runningTask!.IsCompleted);

            Assert.IsTrue(consumerService!._cts!.IsCancellationRequested);

        }

        /// <summary>
        ///  Test that the `StopAsync` method waits for the running task to complete:
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task KafkaConsumerService_StopAsync_WaitsForRunningTask()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                {"Kafka:BootstrapServers", "localhost:9092"},
                {"Kafka:Consumer:GroupId", "test-group"},
                {"Kafka:MaxRetryAttempts", "3"},
                {"Kafka:DlqTopic", "test-dlq"},
                {"Kafka:Consumer:Topic", "test-topic"}
            })
            .Build();
            var loggerFactory = new LoggerFactory();
            var consumerService = new KafkaConsumerService(configuration, loggerFactory);
            var cancellationTokenSource = new CancellationTokenSource();
            await consumerService.StartAsync(cancellationTokenSource.Token);
            // Act
            var stopTask = consumerService.StopAsync(CancellationToken.None);

            // Assert
            Assert.IsFalse(stopTask.IsCompleted);
            Assert.IsFalse(consumerService._runningTask.IsCompleted);

            // Let the running task complete
            await Task.Delay(1000);
            Assert.IsTrue(consumerService._runningTask.IsCompleted);

            // Ensure that the stop task completes after the running task
            await stopTask;
        }
    }

}