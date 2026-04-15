using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Domain.Entities;

namespace Products.Infrastructure.Data.Configurations;

public class SequenceEntryConfiguration : IEntityTypeConfiguration<SequenceEntry>
{
    public void Configure(EntityTypeBuilder<SequenceEntry> builder)
    {
        builder.ToTable("Sequences");

        builder.HasKey(e => e.SequenceType);

        builder.Property(e => e.SequenceType)
            .HasColumnName("Type")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(e => e.SeqNumber)
            .IsRequired();
    }
}
