using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Schema;
using SATS.AI.Documents;
using SATS.AI.Documents.Entities;
using SATS.AI.Models;
using SATS.AI.Utilities;

namespace SATS.AI.Tools;

public class ReadDocumentTool(DocumentDbContext dbContext) : Tool<ReadDocumentByIdQuery, Result<DocumentDto>>
{
    public override string Name => "ReadDocument";
    public override string Description => "Use this when a user requests full content, or when you need to confirm exact wording before updating or responding.";

    public override async Task<Result<DocumentDto>?> ExecuteAsync(ReadDocumentByIdQuery input, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(input.Id) && string.IsNullOrEmpty(input.Path))
            {
                throw new ArgumentException("Either Id or Path must be provided.");
            }

            Document? entity = null;

            if (input.Id is not null)
            {
                var entityId = Guid.Parse(input.Id);
                entity = await dbContext.Documents.FirstOrDefaultAsync(d => d.Id == entityId, cancellationToken);
            }
            else if (input.Path is not null)
            {
                var path = PathHelper.ToLTree(input.Path);
                entity = await dbContext.Documents.FirstOrDefaultAsync(d => d.Path == path, cancellationToken);
            }


            if (entity == null)
            {
                return null;
            }

            var dto = new DocumentDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Content = entity.Content,
                Path = entity.Path
            };

            return Result<DocumentDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<DocumentDto>.Failure($"An error occurred while reading the document: {ex.Message}");
        }
    }

    protected override object GetParameterSchema()
    {
        var schema = new JSchema
        {
            Type = JSchemaType.Object
        };

        schema.Properties.Add("Id", new JSchema { Type = JSchemaType.String });
        schema.Properties.Add("Path", new JSchema { Type = JSchemaType.String });

        return schema;
    }
}

public record ReadDocumentByIdQuery
{
    public string? Id { get; set; }
    public string? Path { get; set; }
}