using Infrastructure.RabbitMQ;

namespace Infraestructura.RabbitMQ
{
    public interface IRabbitMQQueue<T>
    {
        Task QueueAsync(string exchange, string queue, T message);
        Task<DequeueAsyncResponse<T>> DequeueAsync(string queue);
        Task BasicAckAsync(DequeueAsyncResponse<T> response);
        Task BasicNackAsync(DequeueAsyncResponse<T> response, bool requeue);
    }
}