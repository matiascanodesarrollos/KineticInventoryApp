namespace Infrastructure.RabbitMQ
{
    public class InventoryQueueSettings
    {
        public string Exchange { get; set; }
        public string DLX { get; set; }
        public string DLQ { get; set; }
        public string DLQRoutingKey { get; set; }
        public string AddProductQueue { get; set; }
        public string PutProductQueue { get; set; }
        public string DeleteProductQueue { get; set; }
    }
}
