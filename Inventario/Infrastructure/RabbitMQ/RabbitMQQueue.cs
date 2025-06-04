using Infraestructura.RabbitMQ;
using Infrastructure.Polly;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Infrastructure.RabbitMQ
{
    public class RabbitMQQueue<T> : IRabbitMQQueue<T>
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly IPollyResiliencePipeline _resiliencePipelineSingleton;

        public RabbitMQQueue(
            IOptions<RabbitMQSettings> settings,
            IPollyResiliencePipeline resiliencePipelineSingleton)
        {
            _connectionFactory = new ConnectionFactory
            {
                HostName = settings.Value.HostName,
                UserName = settings.Value.UserName,
                Password = settings.Value.Password,
            };
            _resiliencePipelineSingleton = resiliencePipelineSingleton;
        }

        public async Task QueueAsync(string exchange, string queue, T message)
        {
            await _resiliencePipelineSingleton.ExecuteAsync(async (context) =>
            {
                using var connection = await _connectionFactory.CreateConnectionAsync();
                using var channel = await connection.CreateChannelAsync();

                var jsonMessage = JsonSerializer.Serialize(message);
                var bytes = Encoding.UTF8.GetBytes(jsonMessage);

                await channel.BasicPublishAsync(exchange: exchange, routingKey: queue, body: bytes);
            });
        }

        public async Task<DequeueAsyncResponse<T>> DequeueAsync(string queue)
        {
            return await _resiliencePipelineSingleton.ExecuteAsync(async (context) =>
            {
                var response = new DequeueAsyncResponse<T>();
                response.Connection = await _connectionFactory.CreateConnectionAsync();
                response.Channel = await response.Connection.CreateChannelAsync();

                var message = await response.Channel.BasicGetAsync(queue: queue, autoAck: false);
                if (message == null)
                {
                    response.Channel.Dispose();
                    response.Connection.Dispose();
                    return response;
                }

                response.DeliveryTag = message.DeliveryTag;
                response.RawMessage = Encoding.UTF8.GetString(message.Body.ToArray());
                response.Response = JsonSerializer.Deserialize<T>(response.RawMessage);

                return response;
            });
        }

        public async Task BasicAckAsync(DequeueAsyncResponse<T> response)
        {
            using (response.Channel)
            using (response.Connection)
            {
                await _resiliencePipelineSingleton.ExecuteAsync(async (context) =>
                {
                    await response.Channel.BasicAckAsync(response.DeliveryTag, false);
                });
            }
        }

        public async Task BasicNackAsync(DequeueAsyncResponse<T> response, bool requeue)
        {
            using (response.Channel)
            using (response.Connection)
            {
                await _resiliencePipelineSingleton.ExecuteAsync(async (context) =>
                {
                    await response.Channel.BasicNackAsync(response.DeliveryTag, false, requeue);
                });
            }
        }
    }
}
