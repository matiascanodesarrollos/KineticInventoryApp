namespace Infrastructure.Domain
{
    public class ProductJob
    {
        public string Id { get; set; }
        public string ProductId { get; set; }
        public string QueueMessage { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string? Error { get; set; }
    }

    public class ProductJobStatus
    {
        public const string Started = "Started";
        public const string Completed = "Completed";
        public const string Failed = "Failed";
    }

    public class ProductJobType
    {
        public const string Insert = "Insert";
        public const string Update = "Update";
        public const string Delete = "Delete";
    }
}
