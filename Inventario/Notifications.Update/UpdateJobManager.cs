using Infraestructura.RabbitMQ;
using Infrastructure.Domain;
using Infrastructure.EFCoreSqLite;
using Infrastructure.JobManager;
using Infrastructure.RabbitMQ;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Notifications.Update
{
    public class UpdateJobManager : NotificationsJobManager
    {
        public UpdateJobManager(
            ProductsContext productsContext, 
            IRabbitMQQueue<Product> rabbitMQQueue,
            IOptions<InventoryQueueSettings> inventorySettings) : 
            base(productsContext, rabbitMQQueue, inventorySettings.Value.PutProductQueue, ProductJobType.Update)
        {
        }

        protected override async Task SubTask(ProductJob productJob, Product product)
        {
            var databaseRecord = await _productsContext.Products.FirstOrDefaultAsync(p => p.Id == product.Id);
            if (databaseRecord == null)
            {
                throw new Exception($"{DateTime.Now}: Product '{product.Id}' not found.");
            }

            databaseRecord.Nombre = product.Nombre;
            databaseRecord.Descripcion = product.Descripcion;
            databaseRecord.Precio = product.Precio;
            databaseRecord.Stock = product.Stock;
            databaseRecord.Categoria = product.Categoria;
            databaseRecord.ModifiedDateTimeUtc = DateTime.UtcNow;
        }
    }
}
