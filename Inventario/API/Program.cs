using Infrastructure.ConfigurationProvider;
using Infrastructure.EFCoreSqLite;
using Infrastructure.RabbitMQ;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructureServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductsContext>();
    db.Database.Migrate();

    await ConfigureRabbitMq(scope);
}

app.Run();

async Task ConfigureRabbitMq(IServiceScope scope)
{
    var rabbitMQSettings = scope.ServiceProvider.GetRequiredService<IOptions<RabbitMQSettings>>().Value;
    var rabbitMqConnectionFactory = new ConnectionFactory
    {
        HostName = rabbitMQSettings.HostName,
        UserName = rabbitMQSettings.UserName,
        Password = rabbitMQSettings.Password,
    };
    Console.WriteLine(rabbitMQSettings);
    using var connection = await rabbitMqConnectionFactory.CreateConnectionAsync();
    using var channel = await connection.CreateChannelAsync();
    var inventoryQueueSettings = scope.ServiceProvider.GetRequiredService<IOptions<InventoryQueueSettings>>().Value;
    await channel.ExchangeDeclareAsync(exchange: inventoryQueueSettings.Exchange, type: "direct", true);
    await channel.ExchangeDeclareAsync(exchange: inventoryQueueSettings.DLX, type: "direct", true);
    await channel.QueueDeclareAsync(queue: inventoryQueueSettings.DLQ, durable: true, exclusive: false, autoDelete: false);
    await channel.QueueBindAsync(exchange: inventoryQueueSettings.DLX, queue: inventoryQueueSettings.DLQ, routingKey: inventoryQueueSettings.DLQRoutingKey);

    var dlqArgument = new Dictionary<string, object?>() {
        { "x-dead-letter-exchange", "dlx" },
        { "x-dead-letter-routing-key", "dead_letter" },
    };
    await channel.QueueDeclareAsync(queue: inventoryQueueSettings.AddProductQueue, durable: true, exclusive: false, autoDelete: false, arguments: dlqArgument);
    await channel.QueueBindAsync(exchange: inventoryQueueSettings.Exchange, queue: inventoryQueueSettings.AddProductQueue, routingKey: inventoryQueueSettings.AddProductQueue);
    await channel.QueueDeclareAsync(queue: inventoryQueueSettings.DeleteProductQueue, durable: true, exclusive: false, autoDelete: false, arguments: dlqArgument);
    await channel.QueueBindAsync(exchange: inventoryQueueSettings.Exchange, queue: inventoryQueueSettings.DeleteProductQueue, routingKey: inventoryQueueSettings.DeleteProductQueue);
    await channel.QueueDeclareAsync(queue: inventoryQueueSettings.PutProductQueue, durable: true, exclusive: false, autoDelete: false, arguments: dlqArgument);
    await channel.QueueBindAsync(exchange: inventoryQueueSettings.Exchange, queue: inventoryQueueSettings.PutProductQueue, routingKey: inventoryQueueSettings.PutProductQueue);
}
