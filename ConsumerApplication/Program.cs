using ConsumerApplication.Service;

var builder = WebApplication.CreateBuilder(args);

//register the service with the DI container
builder.Services.AddHostedService<KafkaConsumerService>();

// adding a console logger
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
    logging.AddConsole();
});

builder.Services.AddSingleton< IConfiguration >(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapGet("/", () => "Hello from message Consumer!");

app.Run();

