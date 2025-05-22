namespace SATS.AI.Options;

public class PostgresOptions
{
    public required string Host { get; set; }
    public required string Port { get; set; }
    public required string DatabaseName { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }

    public string ConnectionString =>
        $"Host={Host};Port={Port};Database={DatabaseName};Username={Username};Password={Password}";
}
