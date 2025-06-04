using API.Controllers.Producto;
using Infraestructura.RabbitMQ;
using Infrastructure.Domain;
using Infrastructure.EFCoreSqLite;
using Infrastructure.RabbitMQ;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace Inventario.API.Controllers.Producto
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IRabbitMQQueue<Product> _rabbitMQService;
        private readonly InventoryQueueSettings _inventoryQueueSettings;
        private readonly ProductsContext _productsDBContext;

        public ProductsController(
            IRabbitMQQueue<Product> rabbitMQService,
            ProductsContext productsDBContext,
            IOptions<InventoryQueueSettings> inventoryQueueSettings)
        {
            _rabbitMQService = rabbitMQService;
            _productsDBContext = productsDBContext;
            _inventoryQueueSettings = inventoryQueueSettings.Value;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var products = await _productsDBContext.Products.ToListAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var product = await _productsDBContext.Products.FirstOrDefaultAsync(p => p.Id == id);
            return Ok(product);
        }

        [HttpPost]
        [SwaggerResponse(202)]
        public async Task<IActionResult> Post(ProductDto request)
        {
            var product = CreateProduct(request);
            await _rabbitMQService.QueueAsync(
                _inventoryQueueSettings.Exchange, 
                _inventoryQueueSettings.AddProductQueue, 
                product);
            return AcceptedAtAction(nameof(Get),ControllerContext.ActionDescriptor.ControllerName, new { product.Id });
        }

        [HttpPut("{id}")]
        [SwaggerResponse(202)]
        public async Task<IActionResult> Put(string id, [FromBody] ProductDto request)
        {
            var product = CreateProduct(request, id);
            await _rabbitMQService.QueueAsync(
                _inventoryQueueSettings.Exchange, 
                _inventoryQueueSettings.PutProductQueue, 
                product);
            return AcceptedAtAction(nameof(Get), ControllerContext.ActionDescriptor.ControllerName, new { product.Id });
        }

        [HttpDelete("{id}")]
        [SwaggerResponse(202)]
        public async Task<IActionResult> Delete(string id)
        {
            var product = CreateProduct(null, id);
            await _rabbitMQService.QueueAsync(
                _inventoryQueueSettings.Exchange, 
                _inventoryQueueSettings.DeleteProductQueue, 
                product);
            return Accepted();
        }

        private Product CreateProduct(ProductDto? productDto, string? id = null)
        {
            var product = new Product
            {
                Id = id ?? Guid.NewGuid().ToString(),
                ModifiedDateTimeUtc = DateTime.UtcNow,
            };

            if(productDto != null)
            {
                product.Nombre = productDto.Nombre;
                product.Descripcion = productDto.Descripcion;
                product.Precio = productDto.Precio;
                product.Stock = productDto.Stock;
                product.Categoria = productDto.Categoria;
            }
                
            return product;

        }
    }
}