using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Schema;
using SATS.AI.Documents;

namespace SATS.AI.Tools;

public class ReadDocumentByPathTool(DocumentDbContext dbContext) : Tool<ReadDocumentByPathQuery, DocumentDto>
{
    public override string Name => "ReadDocumentById";
    public override string Description => "Use this when a user requests full content, or when you need to confirm exact wording before updating or responding.";

    public override async Task<DocumentDto?> ExecuteAsync(ReadDocumentByPathQuery input, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Documents.FirstOrDefaultAsync(d => d.Path == input.Path, cancellationToken);

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

        schema.Properties.Add("Path", new JSchema { Type = JSchemaType.String });
        schema.Required.Add("Path");

        return schema;
    }
}

public record ReadDocumentByPathQuery
{
    public required string Path { get; set; }
}
