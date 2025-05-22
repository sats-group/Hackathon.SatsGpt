using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using SATS.AI.Documents;
using SATS.AI.Extensions;
using SATS.AI.Models;

namespace SATS.AI.Tools;

public class FindDocumentsInDirectoryTool(DocumentDbContext dbContext) : GenericTool<DocumentDirectoryQuery, List<DocumentDto>>
{
    public override string Name => "Find documents in directory";

    public override string Description => "Searches recursively for documents in parth";

    public override async Task<List<DocumentDto>?> ExecuteAsync(DocumentDirectoryQuery input, CancellationToken cancellationToken)
    {
        var documents = await dbContext.Documents.GetDocumentsUnderPath(input.Path).ToListAsync(cancellationToken);

        return [.. documents.Select(d => new DocumentDto
        {
            Id = d.Id,
            Title = d.Title,
            Content = d.Content,
            Path = d.Path
        })];
    }
}

public record DocumentDirectoryQuery
{
    [Description("An LTree path where each component is delimited by a dot")]
    public required string Path { get; set; }
}