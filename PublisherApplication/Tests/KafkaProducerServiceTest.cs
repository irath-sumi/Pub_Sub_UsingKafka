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
            var messageToPublish = "test-message"; 
            
            var configuration = new ConfigurationBuilder()
           .AddInMemoryCollection(new Dictionary<string, string>()
            {
                {"KafkaProducerConfig:BootstrapServers", "localhost:9092"},
                {"KafkaProducerConfig:Config:message.max.bytes", "1000000"},
                {"KafkaProducerConfig:Config:receive.message.max.bytes","1513486160" }
               
           }).Build();

            // Act  
            var producer = new KafkaProducer(configuration);

            var isMessagePublished = await producer.ProduceAsync(messageToPublish, "TEST-topic");

            // Assert            
            Assert.IsTrue(isMessagePublished);

        }
    }

}
