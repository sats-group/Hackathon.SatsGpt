using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Schema;
using OpenAI.Embeddings;
using Pgvector;
using SATS.AI.Documents;
using SATS.AI.Models;

namespace SATS.AI.Tools;

public class UpdateDocumentTool(
    DocumentDbContext dbContext,
    EmbeddingClient embeddingClient) : Tool<UpdateDocumentCommand, Unit>
{
    public override string Name => "UpdateDocument";
    public override string Description => "Use this to change the title, content, or summary of an existing document. Recalculates the embedding if content changes. Confirm updates with user before applying.";

    public override async Task<Unit?> ExecuteAsync(UpdateDocumentCommand input, CancellationToken cancellationToken)
    {
        if (input.Title is null && input.Content is null && input.Summary is null)
        {
            throw new ArgumentException("At least one of Title, Content, or Summary must be provided.");
        }

        var entity = await dbContext.Documents.FirstOrDefaultAsync(d => d.Id == input.Id, cancellationToken)
            ?? throw new ArgumentException($"Document with ID {input.Id} not found.");

        if (input.Title is not null)
        {
            entity.Title = input.Title;
        }

        if (input.Content is not null)
        {
            entity.Content = input.Content;
            var response = await embeddingClient.GenerateEmbeddingAsync(input.Content, null, cancellationToken);
            var embedding = response.Value.ToFloats().ToArray();
            entity.Embedding = new Vector(embedding);
        }

        if (input.Summary is not null)
        {
            entity.Summary = input.Summary;
        }

        dbContext.Documents.Update(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    protected override object GetParameterSchema()
    {
        var schema = new JSchema
        {
            Type = JSchemaType.Object
        };

        schema.Properties.Add("Id", new JSchema { Type = JSchemaType.String });
        schema.Properties.Add("Title", new JSchema { Type = JSchemaType.String });
        schema.Properties.Add("Content", new JSchema { Type = JSchemaType.String });
        schema.Properties.Add("Summary", new JSchema { Type = JSchemaType.String });

        schema.Required.Add("Id");

        return schema;
    }
}

public record UpdateDocumentCommand
{
    public required Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Summary { get; set; }
}