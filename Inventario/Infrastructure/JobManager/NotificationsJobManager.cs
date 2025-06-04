using Infraestructura.RabbitMQ;
using Infrastructure.Domain;
using Infrastructure.EFCoreSqLite;
using Infrastructure.RabbitMQ;
using Polly.CircuitBreaker;

namespace Infrastructure.JobManager
{
    public abstract class NotificationsJobManager : INotificationsJobManager
    {
        protected readonly ProductsContext _productsContext;
        protected readonly IRabbitMQQueue<Product> _rabbitMQQueue;
        protected readonly string _queue;
        protected readonly string _jobType;

        public NotificationsJobManager(
            ProductsContext productsContext,
            IRabbitMQQueue<Product> rabbitMQQueue,
            string queue,
            string jobType)
        {
            _productsContext = productsContext;
            _rabbitMQQueue = rabbitMQQueue;
            _queue = queue;
            _jobType = jobType;
        }

        protected abstract Task SubTask(ProductJob productJob, Product product);

        public async Task RunJob()
        {
            Console.WriteLine($"{DateTime.Now}: Starting {_jobType} product job.");
            while (true)
            {
                var dequeueResponse = default(DequeueAsyncResponse<Product>);
                try
                {
                    dequeueResponse = await _rabbitMQQueue.DequeueAsync(_queue);
                }
                catch (BrokenCircuitException ex)
                {
                    var waitTime = TimeSpan.FromSeconds(30);
                    Console.WriteLine($"{DateTime.Now}: waiting {waitTime}, Error: {ex}");
                    await Task.Delay(waitTime);
                    continue;
                }

                if (dequeueResponse?.RawMessage == null)
                {
                    var waitTime = TimeSpan.FromMinutes(5);
                    Console.WriteLine($"{DateTime.Now}: No messages were found waiting {waitTime}.");
                    await Task.Delay(waitTime);
                    continue;
                }
                var product = dequeueResponse.Response;
                var productJob = new ProductJob
                {
                    Id = Guid.NewGuid().ToString(),
                    ProductId = product.Id,
                    QueueMessage = dequeueResponse.RawMessage,
                    Status = ProductJobStatus.Started,
                    Type = _jobType,
                };

                try
                {
                    Console.WriteLine($"{DateTime.Now}: Processing job '{productJob.Id}', product '{product.Id}'.");

                    await _productsContext.ProductJobs.AddAsync(productJob);
                    await _productsContext.SaveChangesAsync();

                    await SubTask(productJob, product);

                    productJob.Status = ProductJobStatus.Completed;
                    await _productsContext.SaveChangesAsync();

                    await _rabbitMQQueue.BasicAckAsync(dequeueResponse);

                    Console.WriteLine($"{DateTime.Now}: Completed job '{productJob.Id}', product '{product.Id}'.");
                }
                catch (Exception ex)
                {
                    var errorDetails = ex.ToString();
                    Console.Error.WriteLine($"{DateTime.Now}: Error job '{productJob.Id}', product '{product.Id}'. Error: {errorDetails}");

                    productJob.Status = ProductJobStatus.Failed;
                    productJob.Error = errorDetails;
                    await _productsContext.SaveChangesAsync();
                    await _rabbitMQQueue.BasicNackAsync(dequeueResponse, false);
                }
            }


        }
    }
}
