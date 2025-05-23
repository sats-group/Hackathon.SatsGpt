using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Schema;
using SATS.AI.Documents;
using SATS.AI.Models;

namespace SATS.AI.Tools;

public class RenderFolderTreeTool(DocumentDbContext dbContext)
    : Tool<Unit, List<string>>
{
    public override string Name => "RenderFolderTree";
    public override string Description =>
        "Returns a flat list of all folder paths where documents are stored. " +
        "Use this to understand the directory structure or help the user decide where to read or write documents.";

    public override async Task<List<string>?> ExecuteAsync(Unit input, CancellationToken cancellationToken)
    {
        var allPaths = await dbContext.Documents
            .Select(d => d.Path)
            .Distinct()
            .ToListAsync(cancellationToken);

        var folders = new HashSet<string> { "sats" };

        foreach (var path in allPaths)
        {
            var segments = path.Split('.');
            for (int i = 1; i < segments.Length; i++)
            {
                var folder = string.Join('.', segments.Take(i));
                folders.Add(folder);
            }
        }

        var sortedFolders = folders.OrderBy(p => p).ToList();

        return sortedFolders;
    }

    protected override object? GetParameterSchema()
    {
        return null;
    }
}
