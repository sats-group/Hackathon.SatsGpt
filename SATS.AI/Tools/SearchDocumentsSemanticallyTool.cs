using Microsoft.EntityFrameworkCore;
using OpenAI.Embeddings;
using SATS.AI.Documents;
using SATS.AI.Extensions;
using SATS.AI.Models;

namespace SATS.AI.Tools;

public class SearchDocumentsSemanticallyTool(
    DocumentDbContext dbContext,
    EmbeddingClient embeddingClient) : GenericTool<SemanticQuery, List<DocumentDto>>
{
    public override string Name => "Document Semantic Search";
    public override string Description => "Search for documents semantically. Use this tool to find documents that are similar to a given query.";

    public override async Task<List<DocumentDto>?> ExecuteAsync(SemanticQuery input, CancellationToken cancellationToken)
    {
        var embedding = await embeddingClient.GenerateEmbeddingAsync(input.Query, null, cancellationToken);

        var documents = await dbContext.Documents
            .SearchByEmbedding(embedding.Value.ToFloats().ToArray())
            .ToListAsync(cancellationToken);

        return [.. documents.Select(d => new DocumentDto
        {
            Id = d.Id,
            Title = d.Title,
            Content = d.Content,
            Path = d.Path
        })];
    }
}

public record SemanticQuery
{
    public required string Query { get; set; }
}