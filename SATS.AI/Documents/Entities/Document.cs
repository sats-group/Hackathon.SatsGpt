using Pgvector;

namespace SATS.AI.Documents.Entities;

public class Document
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public required string Path { get; set; }
    public required Vector Embedding { get; set; }
}
