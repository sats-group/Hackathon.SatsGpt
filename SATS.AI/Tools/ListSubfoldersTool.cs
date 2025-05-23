using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Schema;
using Npgsql;
using SATS.AI.Documents;

namespace SATS.AI.Tools;

public class ListSubfoldersTool(DocumentDbContext dbContext)
    : Tool<DocumentDirectoryQuery, List<FolderDto>>
{
    public override string Name => "ListSubfolders";
    public override string Description => """
        Use this to explore the structure of the knowledge base by listing immediate subfolders under a directory. 
        Useful when narrowing down which area to browse, starting from ‘sats’.
    """;

    public override async Task<List<FolderDto>?> ExecuteAsync(DocumentDirectoryQuery input, CancellationToken cancellationToken)
    {
        var sql = """
            SELECT DISTINCT subpath(path, nlevel(@basePath), 1) AS name
            FROM documents
            WHERE path <@ @basePath
              AND nlevel(path) > nlevel(@basePath)
        """;

        var basePath = input.Path;

        var parameters = new[]
        {
            new NpgsqlParameter("basePath", NpgsqlTypes.NpgsqlDbType.LTree) { Value = basePath }
        };

        var folderNames = await dbContext.Database
            .SqlQueryRaw<string>(sql, parameters)
            .ToListAsync(cancellationToken);

        var folderDtos = folderNames
            .Distinct()
            .Select(name => new FolderDto { Name = name, Path = $"{basePath}.{name}" })
            .ToList();

        return folderDtos;
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

public record DocumentDirectoryQuery
{
    public required string Path { get; set; }
}

public record FolderDto
{
    public required string Name { get; set; }
    public required string Path { get; set; }
}
