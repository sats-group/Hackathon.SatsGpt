using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Schema;
using SATS.AI.Documents;
using SATS.AI.Models;

namespace SATS.AI.Tools;

public class DeleteDocumentTool(DocumentDbContext dbContext) : Tool<DeleteDocumentCommand, Unit>
{
    public override string Name => "DeleteDocument";
    public override string Description => "Deletes a document by its ID.";

    public override async Task<Unit?> ExecuteAsync(DeleteDocumentCommand input, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Documents.FirstOrDefaultAsync(d => d.Id == input.Id, cancellationToken)
            ?? throw new ArgumentException($"Document with ID {input.Id} not found.");

        dbContext.Documents.Remove(entity);
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
        schema.Required.Add("Id");

        return schema;
    }
}

public record DeleteDocumentCommand
{
    public required Guid Id { get; set; }
}
