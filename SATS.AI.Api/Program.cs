using SATS.AI.Options;
using SATS.AI.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var options = builder.Configuration.GetSection("OpenAI").Get<OpenAIOptions>();
builder.Services.AddAI(options!);

var app = builder.Build();
app.MapControllers();
app.Run();