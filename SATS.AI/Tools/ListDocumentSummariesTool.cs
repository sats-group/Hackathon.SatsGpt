using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Schema;
using SATS.AI.Documents;
using SATS.AI.Extensions;
using SATS.AI.Models;
using SATS.AI.Utilities;

namespace SATS.AI.Tools;

public class ListDocumentSummariesTool(DocumentDbContext dbContext)
    : Tool<DirectoryListingQuery, Result<List<DirectoryEntry>>>
{
    public override string Name => "ListDocumentSummaries";
    public override string Description => """
        Use this to quickly get summaries of all documents under a given path. 
        Best for skimming content without reading entire documents.
    """;

    public override async Task<Result<List<DirectoryEntry>>?> ExecuteAsync(DirectoryListingQuery input, CancellationToken cancellationToken)
    {
        try
        {
            var path = PathHelper.ToLTree(input.Path);

            var entities = await dbContext
                .ListDocuments(path)
                .ToListAsync(cancellationToken);

            var docs = entities
                .Select(d => new DirectoryEntry { Title = d.Title, Path = PathHelper.FromLTree(d.Path), IsDocument = true })
                .Distinct()
                .ToList();

            var subfolders = docs
                .Select(d => d.Path[(input.Path.Length + 1)..].Split('.').First())
                .Distinct()
                .Select(name => new DirectoryEntry { Title = name, Path = PathHelper.FromLTree($"{input.Path}.{name}"), IsDocument = false })
                .ToList();

            return Result<List<DirectoryEntry>>.Success([.. subfolders, .. docs]);
        }
        catch (Exception ex)
        {
            return Result<List<DirectoryEntry>>.Failure($"An error occurred while listing the directory: {ex.Message}");
        }
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

public record DocumentSummaryDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Summary { get; set; }
    public required string Path { get; set; }
}
