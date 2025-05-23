using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Schema;
using SATS.AI.Documents;
using SATS.AI.Extensions;
using SATS.AI.Models;
using SATS.AI.Utilities;

namespace SATS.AI.Tools;

public class ListDirectoryTool(DocumentDbContext dbContext)
    : Tool<DirectoryListingQuery, Result<List<DirectoryEntry>>>
{
    public override string Name => "ListDirectory";
    public override string Description => """
        Use this to view all documents and folders within a given directory path. 
        Helpful when users refer to something specific (e.g., “How do I make a new release of the iOS app?”).
    """;

    public override async Task<Result<List<DirectoryEntry>>?> ExecuteAsync(DirectoryListingQuery input, CancellationToken cancellationToken)
    {
        try
        {
            var path = PathHelper.ToLTree(input.Path);

            var docs = await dbContext
                .ListDocuments(path)
                .Select(d => new DirectoryEntry { Title = d.Title, Path = PathHelper.FromLTree(d.Path), IsDocument = true })
                .Distinct()
                .ToListAsync(cancellationToken);

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
