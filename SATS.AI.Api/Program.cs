using SATS.AI.Options;
using SATS.AI.Extensions;
using SATS.AI.Schema;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

var openAIoptions = builder.Configuration.GetSection("OpenAI").Get<OpenAIOptions>();
builder.Services.AddChat(openAIoptions!);

var postgresOptions = builder.Configuration.GetSection("Postgres").Get<PostgresOptions>();
builder.Services.AddDocumentStore(postgresOptions!);

var app = builder.Build();
app.Services.ApplyPendingMigrations();
app.MapControllers();
app.Run();