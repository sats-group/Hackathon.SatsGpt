using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using SATS.AI.Chat;
using SATS.AI.Options;
using SATS.AI.Runners;
using SATS.AI.Tools;

namespace SATS.AI.Extensions;

public static class AIServiceCollectionExtensions
{
    public static IServiceCollection AddAI(this IServiceCollection services, OpenAIOptions options)
    {
        services
            .AddSingleton(new OpenAIClient(options.ApiKey))
            .AddSingleton(sp =>
            {
                var client = sp.GetRequiredService<OpenAIClient>();
                var tools = sp.GetServices<ITool>();
                return new AgentRunner(client.GetChatClient("gpt-4o"), tools);
            })
            .AddSingleton<ITool, TestTool>()
            .AddSingleton<ChatStore>();

        return services;
    }
}
