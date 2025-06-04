using RabbitMQ.Client;

namespace Infrastructure.RabbitMQ
{
    public class DequeueAsyncResponse<T>
    {
        public string RawMessage { get; set; }
        public T? Response { get; set; }
        public ulong DeliveryTag { get; set; }
        public IChannel Channel { get; set; }
        public IConnection Connection { get; set; }
    }
}
