using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Schema;
using SATS.AI.Documents;
using SATS.AI.Extensions;

namespace SATS.AI.Tools;

public class ListDirectoryTool(DocumentDbContext dbContext)
    : Tool<DirectoryListingQuery, List<DirectoryEntry>>
{
    public override string Name => "ListDirectory";
    public override string Description => """
        Use this to view all documents and folders within a given directory path. 
        Helpful when users refer to something specific (e.g., “How do I make a new release of the iOS app?”).
    """;

    public override async Task<List<DirectoryEntry>?> ExecuteAsync(DirectoryListingQuery input, CancellationToken cancellationToken)
    {
        var docs = await dbContext
            .ListDocuments(input.Path)
            .Select(d => new DirectoryEntry { Title = d.Title, Path = d.Path, IsDocument = true })
            .Distinct()
            .ToListAsync(cancellationToken);

        var subfolders = docs
            .Select(d => d.Path[(input.Path.Length + 1)..].Split('.').First())
            .Distinct()
            .Select(name => new DirectoryEntry { Title = name, Path = $"{input.Path}.{name}", IsDocument = false })
            .ToList();

        return [.. subfolders, .. docs];
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

public record DirectoryListingQuery
{
    public required string Path { get; set; }
}

public record DirectoryEntry
{
    public required string Title { get; set; }
    public required string Path { get; set; }
    public required bool IsDocument { get; set; }
}
