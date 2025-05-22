using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using SATS.AI.Chat;
using SATS.AI.Documents;
using SATS.AI.Options;
using SATS.AI.Runners;
using SATS.AI.Tools;

namespace SATS.AI.Extensions;

public static class AIServiceCollectionExtensions
{
    public static IServiceCollection AddChat(this IServiceCollection services, OpenAIOptions options)
    {
        services
            .AddSingleton(new OpenAIClient(options.ApiKey))
            .AddSingleton(sp =>
            {
                var client = sp.GetRequiredService<OpenAIClient>();
                return client.GetEmbeddingClient("text-embedding-3-small");
            })
            .AddSingleton(sp =>
            {
                var client = sp.GetRequiredService<OpenAIClient>();
                var tools = sp.GetServices<ITool>() ?? [];
                return new AgentRunner(client.GetChatClient("gpt-4o"), tools);
            })
            .AddSingleton<ChatStore>()
            .AddSingleton<ChatNameProvider>();

        return services;
    }
    
    public static IServiceCollection AddDocumentStore(this IServiceCollection services, PostgresOptions options)
    {
        services
            .AddDbContext<DocumentDbContext>(o =>
                o.UseNpgsql(options.ConnectionString, npgsqlOptions =>
                {
                    npgsqlOptions.UseVector();
                }));

        return services;
    }
}
