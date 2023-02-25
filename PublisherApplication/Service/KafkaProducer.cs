namespace PublisherApplication.Service
{
    using Confluent.Kafka;
    using System;
    using System.Threading.Tasks;

    class KafkaProducer
    {
        // public IProducer<string, string> producer;
        private readonly IConfiguration _configuration;
        private readonly IProducer<string, string> producer;
        private readonly string topicName;

        public KafkaProducer(IConfiguration configuration)
        {
            _configuration = configuration;

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = _configuration["KafkaProducerConfig:BootstrapServers"]
            };
            producerConfig.Set("message.max.bytes", _configuration["KafkaProducerConfig:Config:message.max.bytes"]);
            producerConfig.Set("receive.message.max.bytes", _configuration["KafkaProducerConfig:Config:receive.message.max.bytes"]);

            producer = new ProducerBuilder<string, string>(producerConfig).Build();
            topicName = _configuration["KafkaProducerConfig:Topics:Topic1"];
        }

        //// Create a producer and send messages every 20ms
        public async Task ProduceAsync(string message)
        {
            try
            {
                int count = 0;
                while (true)
                {
                    var result = await producer.ProduceAsync(topicName, new Message<string, string>
                    {
                        Key = "publisher-key",
                        Value = message + count.ToString()
                    });
                    if (result.Status == PersistenceStatus.Persisted)
                        Console.WriteLine($"Produced message '{result.Value}' to topic {result.Topic}, partition {result.Partition}, offset {result.Offset}");
                    
                    else                    
                        Console.WriteLine($"Message delivery failed: {result.Status}");
                    

                    Thread.Sleep(20);
                    count++;
                    if (count == 5)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }

}
