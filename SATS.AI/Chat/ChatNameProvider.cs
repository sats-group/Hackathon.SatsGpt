using OpenAI.Chat;
using SATS.AI.Runners;

namespace SATS.AI.Chat;

public class ChatNameProvider(AgentRunner runner)
{
    public async Task<string> GenerateChatName(string message)
    {
        var chatMessages = new List<ChatMessage>
        {
            ChatMessage.CreateSystemMessage(@"
                Your task is to read the first messages of a chat and suggest a name for the chat.
                The name should be short and descriptive, ideally one to three words.
            "),
            ChatMessage.CreateUserMessage(@$"
                Please suggest a name for the chat based on the following messages: {message}
            ")
        };

        var response = await runner.RunAsync<ChatNameSuggestion>(chatMessages);
        return response.SuggestedName;
    }
}

public class ChatNameSuggestion
{
    public required string SuggestedName { get; set; }
}