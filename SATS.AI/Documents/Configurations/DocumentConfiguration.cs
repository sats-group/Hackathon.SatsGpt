using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SATS.AI.Documents.Entities;

namespace SATS.AI.Documents.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("documents");

        builder
            .Property(d => d.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder
            .Property(d => d.Title)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("title");

        builder
            .Property(d => d.Content)
            .IsRequired()
            .HasColumnName("content");

        builder
            .Property(d => d.Path)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnType("ltree")
            .HasColumnName("path");

        builder
            .Property(d => d.Embedding)
            .IsRequired()
            .HasColumnType("vector(1536)")
            .HasColumnName("embedding");
    }
}
