using Microsoft.EntityFrameworkCore;
using Npgsql;
using SATS.AI.Documents;
using SATS.AI.Documents.Entities;

namespace SATS.AI.Extensions;

public static class DbSetExtensions
{
    public static IQueryable<Document> ListDocuments(this DocumentDbContext dbContext, string path)
    {
        const string sql = """
            SELECT * FROM documents
            WHERE path <@ @ltreePath
            AND nlevel(path) = nlevel(@ltreePath) + 1
        """;

        var param = new NpgsqlParameter("ltreePath", NpgsqlTypes.NpgsqlDbType.LTree) { Value = path };

        return dbContext.Documents.FromSqlRaw(sql, param).AsNoTracking();
    }

    public static IQueryable<Document> ListDocumentsRecursively(this DocumentDbContext dbContext, string path)
    {
        var sql = @"
            SELECT * FROM documents
            WHERE path <@ @ltreePath
        ";

        var parameters = new[]
        {
            new NpgsqlParameter("ltreePath", NpgsqlTypes.NpgsqlDbType.LTree) { Value = path }
        };

        return dbContext.Documents.FromSqlRaw(sql, parameters)
            .AsNoTracking()
            .AsQueryable();
    }

    public static Task<List<string>> ListDirectoriesAsync(this DocumentDbContext dbContext, string path, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT DISTINCT subpath(path, nlevel(@basePath), 1) AS folder
            FROM documents
            WHERE path <@ @basePath
            AND nlevel(path) > nlevel(@basePath)
        """;

        var param = new NpgsqlParameter("basePath", NpgsqlTypes.NpgsqlDbType.LTree) { Value = path };

        return dbContext.Database
            .SqlQueryRaw<string>(sql, param)
            .ToListAsync(cancellationToken);
    }

    public static IQueryable<Document> SearchDocuments(this DocumentDbContext dbContext, float[] embedding, int limit = 5)
    {
        var sql = """
            SELECT *
            FROM documents
            WHERE (1 - (embedding <=> @queryEmbedding::vector)) > 0.5
            ORDER BY embedding <=> @queryEmbedding::vector
            LIMIT @topK
        """;

        var parameters = new[]
        {
            new NpgsqlParameter("queryEmbedding", embedding),
            new NpgsqlParameter("topK", limit)
        };

        return dbContext.Documents.FromSqlRaw(sql, parameters)
            .AsNoTracking()
            .AsQueryable();
    }
}
