using Infraestructura.RabbitMQ;
using Infrastructure.Domain;
using Infrastructure.EFCoreSqLite;
using Infrastructure.JobManager;
using Infrastructure.RabbitMQ;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Notifications.Delete
{
    public class DeleteJobManager : NotificationsJobManager
    {
        public DeleteJobManager(
            ProductsContext productsContext,
            IRabbitMQQueue<Product> rabbitMQQueue,
            IOptions<InventoryQueueSettings> inventorySettings)
            : base(productsContext, rabbitMQQueue, inventorySettings.Value.DeleteProductQueue, ProductJobType.Delete)
        {
        }

        protected override async Task SubTask(ProductJob productJob, Product product)
        {
            var databaseRecord = await _productsContext.Products.FirstOrDefaultAsync(p => p.Id == product.Id);
            if (databaseRecord == null)
            {
                throw new Exception($"{DateTime.Now}: Product '{product.Id}' not found.");
            }

            _productsContext.Products.Remove(databaseRecord);
        }
    }
}
