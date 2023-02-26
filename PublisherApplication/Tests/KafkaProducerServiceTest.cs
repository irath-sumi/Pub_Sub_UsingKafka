using Microsoft.VisualStudio.TestTools.UnitTesting;
using PublisherApplication.Service;

namespace PublisherApplication.Tests
{
    [TestClass]
    public class KafkaProducerTests
    {
        [TestMethod]
        public async Task ProduceAsyncShouldProduceMessage()
        {
            // Arrange
            var messageToPublish = "test-message"; var topicName = "test-topic11";
            var config = new ConfigurationBuilder()
                 .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                 .Build();
            // Act
            var producer = new KafkaProducer(config);

            var isMessagePublished = await producer.ProduceAsync(messageToPublish, topicName);

            // Assert            
            Assert.IsTrue(isMessagePublished);

        }
    }

}
