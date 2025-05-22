using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using SATS.AI.Options;

namespace SATS.AI.Documents;

public class DocumentDbContextFactory : IDesignTimeDbContextFactory<DocumentDbContext>
{
    public DocumentDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<DocumentDbContext>();
        var options = configuration.GetSection("Postgres").Get<PostgresOptions>()
            ?? throw new InvalidOperationException("Postgres options not found in configuration.");

        var connectionString = options.ConnectionString;
        
        optionsBuilder.UseNpgsql(connectionString, o =>
        {
            o.UseVector();
        });

        return new DocumentDbContext(optionsBuilder.Options);
    }
}
