using PublisherApplication.Service;

var builder = WebApplication.CreateBuilder(args);

var message = "{ \"id\": 1, \"name\": \"I am the publisher Application on Docker Host\" }";

var topicName = builder.Configuration["KafkaProducerConfig:Topics:Topic1"];

var producer = new KafkaProducer(builder.Configuration);

var isSuccess = await producer.ProduceAsync(message,topicName);
if (isSuccess)
    Console.WriteLine("Message published successfully!");
else
    Console.WriteLine("Error:Message Not published!");

Console.WriteLine("Message published successfully!");

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapGet("/", () => "Hello from message Publisher!");

app.Run();
