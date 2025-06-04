using Infrastructure.Domain;
using Infrastructure.EFCoreSqLite.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EFCoreSqLite
{
    public class ProductsContext : DbContext
    {
        public ProductsContext(DbContextOptions<ProductsContext> options) : base(options) { }

        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ProductJob> ProductJobs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ProductConfiguration());
            modelBuilder.ApplyConfiguration(new ProductJobConfiguration());
        }
    }
}
