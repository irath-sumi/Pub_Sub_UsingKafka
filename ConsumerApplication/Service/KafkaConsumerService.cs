using Confluent.Kafka;
using Polly;
using System.Text;

namespace ConsumerApplication.Service
{
    public class KafkaConsumerService : IHostedService
    {
        private readonly IConfiguration _configuration;
        public readonly IConsumer<Ignore, string> _consumer;
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaConsumerService> _logger;

        private readonly int _maxRetryAttempts;
        private readonly string _dlqTopic;

        public Task? _runningTask;
        public CancellationTokenSource? _cts;

        public KafkaConsumerService(IConfiguration configuration, ILoggerFactory logger)
        {
            _configuration = configuration;
            _logger = logger.CreateLogger<KafkaConsumerService>();

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"],
                GroupId = _configuration["Kafka:Consumer:GroupId"],
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig)
                .SetErrorHandler((_, e) => _logger.LogError($"Kafka error: {e.Reason}"))
                .Build();

            // DLQ Producer 
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"],
                ClientId = "consumer-producer"
            };
            _producer = new ProducerBuilder<string, string>(producerConfig)
                 .Build();

            _maxRetryAttempts = int.Parse(_configuration["Kafka:MaxRetryAttempts"]);
            _dlqTopic = _configuration["Kafka:DlqTopic"];
        }
        /// <summary>
        /// Starts a consumer that subscribes to a Kafka topic.
        /// </summary>
        /// <param name="cancellationToken">this param can be used to cancel the operation if needed.</param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _consumer.Subscribe(_configuration["Kafka:Consumer:Topic"]);

            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _runningTask = Task.Run(() => RunAsync(_cts.Token));

            return Task.CompletedTask;
        }
        /// <summary>
        /// Asynchronous operation to stop the Kafka consumer service that 
        /// was started with the StartAsync method.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Kafka consumer service...");

            _consumer.Close();

            try
            {
                // Signal cancellation to the running task
                _cts!.Cancel();
            }
            finally
            {
                // Wait until the running task completes or the timeout expires
                await Task.WhenAny(_runningTask!, Task.Delay(Timeout.Infinite, cancellationToken));
            }

        }
        /// <summary>
        /// consumes messages from a Kafka topic, process them asynchronously.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(cancellationToken);
                    while (consumeResult != null)
                    {
                        // Process the message
                        var response = await CallApiAsync(consumeResult.Message.Value, cancellationToken);


                        if (response)
                        {
                            Console.WriteLine($"Received message: {consumeResult.Message.Value}");
                            _consumer.Commit(consumeResult);
                            _logger.LogInformation($"Message with key {consumeResult.Message.Key} processed successfully");
                        }
                        else
                        {
                            _logger.LogError($"Failed to process message with key {consumeResult.Message.Key} after {_maxRetryAttempts} retries. Sending to DLQ");

                            var result = await _producer.ProduceAsync(_dlqTopic, new Message<string, string>
                            {
                                Key = "my-DLQ-Topic-message",
                                Value = consumeResult.Message.Value
                            });
                            Console.WriteLine($"Produced message '{result.Value}' to DLQ topic {result.Topic}, partition {result.Partition}, offset {result.Offset}");

                        }
                        // Read the next message
                        consumeResult = _consumer.Consume(cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Consumer is stopping");
                    // Close the consumer gracefully
                    _consumer.Close();
                    break;
                }
                catch (ConsumeException ex)
                {
                    _logger.LogInformation(ex.Message);
                }
            }
        }
        /// <summary>
        /// Calls external (mock) API. Handles the possibility of errors by retrying the operation.
        /// Used the Polly library to implement a retry policy
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> CallApiAsync(string message, CancellationToken cancellationToken)
        {
            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(3, retryCount => TimeSpan.FromSeconds(Math.Pow(2, retryCount)));

            var response = await retryPolicy.ExecuteAsync(async () =>
            {
                using var httpClient = new HttpClient();
                var content = new StringContent(message, Encoding.UTF8, "application/json");
                var httpResponse = await httpClient.PostAsync("https://mockbin.com/request", content, cancellationToken);
                return httpResponse;
            });

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"API call failed with status code {response.StatusCode}");
                return false;
            }

            return true;
        }

    }
}
