using Infraestructura.RabbitMQ;
using Infrastructure.Domain;
using Infrastructure.EFCoreSqLite;
using Infrastructure.Polly;
using Infrastructure.RabbitMQ;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace Infrastructure.ConfigurationProvider
{
    public static class ConfigurationProvider
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            
            services.AddTransient(sp => Options.Create(new RabbitMQSettings
            {
                HostName = "rabbitmq",
                UserName = "guest",
                Password = "guest",
            }));
            services.AddTransient(sp => Options.Create(new InventoryQueueSettings
            {
                Exchange = "inventory_exchange",
                DLX = "dlx",
                DLQ = "dead_letter_queue",
                DLQRoutingKey = "dead_letter",
                AddProductQueue = "add_product",
                PutProductQueue = "put_product",
                DeleteProductQueue = "delete_product",
            }));
            var sqliteConnectionString = "Data Source=/usr/sqlite/product.db";
            services.AddTransient(sp => Options.Create(new EFCoreSqlLiteSettings
            {
                ConnectionString = sqliteConnectionString,
            }));
            services.AddDbContext<ProductsContext>(options => options.UseSqlite(sqliteConnectionString));
            services.AddTransient<IRabbitMQQueue<Product>, RabbitMQQueue<Product>>();
            services.AddSingleton<IPollyResiliencePipeline>(context => new PollyResiliencePipeline(
                new ResiliencePipelineBuilder()
                        .AddRetry(new RetryStrategyOptions
                        {
                            ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                            BackoffType = DelayBackoffType.Exponential,
                            MaxRetryAttempts = 4,
                            Delay = TimeSpan.FromSeconds(3),
                        })
                        .AddCircuitBreaker(new CircuitBreakerStrategyOptions
                        {
                            FailureRatio = 0.1,
                            SamplingDuration = TimeSpan.FromSeconds(1),
                            MinimumThroughput = 10,
                            BreakDuration = TimeSpan.FromSeconds(30),
                            ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                        }))
            );
            return services;
        }
    }
}
