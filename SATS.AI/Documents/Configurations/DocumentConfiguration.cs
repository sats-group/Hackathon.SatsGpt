using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SATS.AI.Documents.Entities;

namespace SATS.AI.Documents.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder
            .Property(d => d.Id)
            .ValueGeneratedNever();

        builder
            .Property(d => d.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder
            .Property(d => d.Content)
            .IsRequired();

        builder
            .Property(d => d.Path)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("ltree");

        builder
            .Property(d => d.Embedding)
            .IsRequired()
            .HasColumnType("vector(1536)");
    }
}
