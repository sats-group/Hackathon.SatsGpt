using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Schema;
using SATS.AI.Documents;
using SATS.AI.Documents.Entities;
using SATS.AI.Models;
using SATS.AI.Utilities;

namespace SATS.AI.Tools;

public class DeleteDocumentTool(DocumentDbContext dbContext) : Tool<DeleteDocumentByIdCommand, Result<Unit>>
{
    public override string Name => "DeleteDocument";
    public override string Description => "Deletes a document by its ID or path.";

    public override async Task<Result<Unit>?> ExecuteAsync(DeleteDocumentByIdCommand input, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(input.Id) && string.IsNullOrEmpty(input.Path))
            {
                return Result<Unit>.Failure("Either Id or Path must be provided.");
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
                return Result<Unit>.Failure("Document not found.");
            }

            dbContext.Documents.Remove(entity!);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result<Unit>.Failure($"An error occurred while deleting the document: {ex.Message}");
        }
    }

    protected override object GetParameterSchema()
    {
        var schema = new JSchema
        {
            Type = JSchemaType.Object,
            Description = "Use either the ID or path to delete a document. If both are provided, the ID will be used."
        };

        schema.Properties.Add("Id", new JSchema { Type = JSchemaType.String });
        schema.Properties.Add("Path", new JSchema { Type = JSchemaType.String });

        return schema;
    }
}

public record DeleteDocumentByIdCommand
{
    public string? Id { get; set; }
    public string? Path { get; set; }
}
