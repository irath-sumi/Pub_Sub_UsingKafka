using PublisherApplication.Service;

var builder = WebApplication.CreateBuilder(args);

var message = "{ \"id\": 1, \"name\": \"I am the publisher Application on Docker Host\" }";


var producer = new KafkaProducer(builder.Configuration);

await producer.ProduceAsync(message);

Console.WriteLine("Message published successfully!");

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapGet("/", () => "Hello from message Publisher!");

app.Run();
