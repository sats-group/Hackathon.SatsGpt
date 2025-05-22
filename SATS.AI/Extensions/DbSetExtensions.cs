using Microsoft.EntityFrameworkCore;
using Npgsql;
using SATS.AI.Documents.Entities;

namespace SATS.AI.Extensions;

public static class DbSetExtensions
{
    public static IQueryable<Document> GetDocumentsUnderPath(this DbSet<Document> documentSet, string path)
    {
        var sql = @"
            SELECT * FROM documents
            WHERE path <@ @ltreePath
        ";

        var parameters = new[]
        {
            new NpgsqlParameter("ltreePath", NpgsqlTypes.NpgsqlDbType.LTree) { Value = path }
        };

        return documentSet.FromSqlRaw(sql, parameters)
            .AsNoTracking()
            .AsQueryable();
    }

    public static IQueryable<Document> SearchByEmbedding(this DbSet<Document> documentSet, float[] embedding, int limit = 5)
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

        return documentSet.FromSqlRaw(sql, parameters)
            .AsNoTracking()
            .AsQueryable();
    }
}
