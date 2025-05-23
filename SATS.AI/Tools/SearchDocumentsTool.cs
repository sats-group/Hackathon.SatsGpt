using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Schema;
using OpenAI.Embeddings;
using SATS.AI.Documents;
using SATS.AI.Extensions;
using SATS.AI.Models;
using SATS.AI.Utilities;

namespace SATS.AI.Tools;

public class SearchDocumentsTool(
    DocumentDbContext dbContext,
    EmbeddingClient embeddingClient) : Tool<SearchDocumentsQuery, Result<List<DocumentDto>>>
{
    public override string Name => "SearchDocuments";
    public override string Description => """
        Use this to find relevant documents based on meaning, not just keywords. 
        Best used for broad or loosely phrased user questions like “How do I handle sick leave?” or “What’s the process for club onboarding?”
    """;

    public override async Task<Result<List<DocumentDto>>?> ExecuteAsync(SearchDocumentsQuery input, CancellationToken cancellationToken)
    {
        try
        {
            var embedding = await embeddingClient.GenerateEmbeddingAsync(input.Query, null, cancellationToken);

            var documents = await dbContext
                .SearchDocuments(embedding.Value.ToFloats().ToArray())
                .ToListAsync(cancellationToken);

            List<DocumentDto> dtos = [.. documents.Select(d => new DocumentDto
            {
                Id = d.Id,
                Title = d.Title,
                Content = d.Content,
                Path = PathHelper.FromLTree(d.Path)
            })];

            return Result<List<DocumentDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<DocumentDto>>.Failure($"An error occurred while searching for documents: {ex.Message}");
        }
    }

    protected override object GetParameterSchema()
    {
        var schema = new JSchema
        {
            Type = JSchemaType.Object
        };

        schema.Properties.Add("Query", new JSchema { Type = JSchemaType.String });
        schema.Required.Add("Query");

        return schema;
    }
}

public record SearchDocumentsQuery
{
    public required string Query { get; set; }
}

public record DocumentDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public required string Path { get; set; }
}