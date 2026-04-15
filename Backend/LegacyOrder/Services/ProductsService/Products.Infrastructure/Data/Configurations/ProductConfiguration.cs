using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Domain.Entities;

namespace Products.Infrastructure.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            
            builder.ToTable("Products");

            
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .ValueGeneratedOnAdd();

            
            builder.Property(p => p.Code)
                .IsRequired()
                .HasMaxLength(32);

            builder.HasIndex(p => p.Code)
                .IsUnique();

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
            
            builder.Property(p => p.Description)
                .HasMaxLength(1000);

            // To prevent Concurrency Issues
            builder.Property<byte[]>("RowVersion")
                .IsRowVersion();

            builder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd();

            builder.Property(p => p.ModifiedAt);

            builder.Property(p => p.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasQueryFilter(p => !p.IsDeleted);
        }
    }
}