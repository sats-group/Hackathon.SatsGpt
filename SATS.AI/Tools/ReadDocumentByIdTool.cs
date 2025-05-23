using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Schema;
using SATS.AI.Documents;

namespace SATS.AI.Tools;

public class ReadDocumentByIdTool(DocumentDbContext dbContext) : Tool<ReadDocumentByIdQuery, DocumentDto>
{
    public override string Name => "ReadDocumentById";
    public override string Description => "Use this when a user requests full content, or when you need to confirm exact wording before updating or responding.";

    public override async Task<DocumentDto?> ExecuteAsync(ReadDocumentByIdQuery input, CancellationToken cancellationToken)
    {
        var docId = Guid.Parse(input.Id);
        var entity = await dbContext.Documents.FirstOrDefaultAsync(d => d.Id == docId, cancellationToken);

        if (entity == null)
        {
            return null;
        }

        return new DocumentDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Content = entity.Content,
            Path = entity.Path
        };
    }

    protected override object GetParameterSchema()
    {
        var schema = new JSchema
        {
            Type = JSchemaType.Object
        };

        schema.Properties.Add("Id", new JSchema { Type = JSchemaType.String });
        schema.Required.Add("Id");

        return schema;
    }
}

public record ReadDocumentByIdQuery
{
    public required string Id { get; set; }
}