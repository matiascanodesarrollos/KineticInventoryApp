using Infraestructura.RabbitMQ;
using Infrastructure.Domain;
using Infrastructure.EFCoreSqLite;
using Infrastructure.JobManager;
using Infrastructure.RabbitMQ;
using Microsoft.Extensions.Options;

namespace Notifications.Insert
{
    public class InsertJobManager : NotificationsJobManager
    {
        public InsertJobManager(
            ProductsContext productsContext, 
            IRabbitMQQueue<Product> rabbitMQQueue, 
            IOptions<InventoryQueueSettings> inventorySettings) 
            : base(productsContext, rabbitMQQueue, inventorySettings.Value.AddProductQueue, ProductJobType.Insert)
        {
        }

        protected override async Task SubTask(ProductJob productJob, Product product)
        {
            await _productsContext.Products.AddAsync(product);
        }
    }
}
