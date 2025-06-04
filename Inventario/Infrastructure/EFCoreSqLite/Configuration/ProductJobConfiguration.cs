using Infrastructure.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.EFCoreSqLite.Configuration
{
    internal class ProductJobConfiguration : IEntityTypeConfiguration<ProductJob>
    {
        public void Configure(EntityTypeBuilder<ProductJob> builder)
        {
            builder.ToTable("ProductJobs");

            builder.HasKey(e => e.Id);
        }
    }
}