using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Schema;
using OpenAI.Embeddings;
using Pgvector;
using SATS.AI.Documents;
using SATS.AI.Documents.Entities;
using SATS.AI.Models;

namespace SATS.AI.Tools;

public class CreateDocumentTool(
    DocumentDbContext dbContext,
    EmbeddingClient embeddingClient) : Tool<CreateDocumentCommand, Unit>
{
    public override string Name => "CreateDocument";
    public override string Description => "Adds a new document with a generated embedding.";

    public override async Task<Unit?> ExecuteAsync(CreateDocumentCommand input, CancellationToken cancellationToken)
    {
        var response = await embeddingClient.GenerateEmbeddingAsync(input.Content, null, cancellationToken);
        var embedding = response.Value.ToFloats().ToArray();

        var document = new Document
        {
            Id = Guid.NewGuid(),
            Title = input.Title,
            Content = input.Content,
            Summary = input.Summary,
            Path = input.Path,
            Embedding = new Vector(embedding)
        };

        dbContext.Documents.Add(document);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    protected override object GetParameterSchema()
    {
        var schema = new JSchema
        {
            Type = JSchemaType.Object
        };

        schema.Properties.Add("Title", new JSchema { Type = JSchemaType.String });
        schema.Properties.Add("Content", new JSchema { Type = JSchemaType.String });
        schema.Properties.Add("Summary", new JSchema { Type = JSchemaType.String });
        schema.Properties.Add("Path", new JSchema { Type = JSchemaType.String });

        schema.Required.Add("Title");
        schema.Required.Add("Content");
        schema.Required.Add("Summary");
        schema.Required.Add("Path");

        return schema;
    }
}

public record CreateDocumentCommand
{
    [Required]
    public required string Title { get; set; }

    [Required]
    public required string Content { get; set; }

    [Required]
    public required string Summary { get; set; }

    [Required]
    public required string Path { get; set; }
}
