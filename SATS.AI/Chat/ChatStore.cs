using Microsoft.Extensions.Caching.Memory;
using OpenAI.Chat;

namespace SATS.AI.Chat;

public class ChatStore(IMemoryCache memoryCache)
{
    public List<ChatMessage> Get(string key)
    {
        if (memoryCache.TryGetValue(key, out List<CachedChatMessage>? cachedMessages))
        {
            return ToChatMessages(cachedMessages!);
        }

        return [
            ChatMessage.CreateSystemMessage(@"
                You are a helpful assistant. You will answer questions with a focus on clarity and brevity. Try and keep
                responses to a maximum of 3 sentences. If you don't know the answer, say 'I don't know'.
            ")
        ];
    }

    public void Set(string key, List<ChatMessage> messages)
    {
        var cachedMessages = ToCachedChatMessages(messages);
        memoryCache.Set(key, cachedMessages);
    }

    private static List<CachedChatMessage> ToCachedChatMessages(List<ChatMessage> messages)
    {
        var result = new List<CachedChatMessage>();

        foreach (var message in messages)
        {
            var cachedMessage = message switch
            {
                UserChatMessage userMessage => new CachedChatMessage
                {
                    Role = ChatMessageRole.User,
                    Content = ExtractContentText(userMessage.Content),
                    ToolCallId = null
                },
                AssistantChatMessage assistantChatMessage => new CachedChatMessage
                {
                    Role = ChatMessageRole.Assistant,
                    Content = ExtractContentText(assistantChatMessage.Content),
                    ToolCallId = assistantChatMessage.ToolCalls.FirstOrDefault()?.Id
                },
                SystemChatMessage systemChatMessage => new CachedChatMessage
                {
                    Role = ChatMessageRole.System,
                    Content = ExtractContentText(systemChatMessage.Content),
                    ToolCallId = null
                },
                ToolChatMessage toolChatMessage => new CachedChatMessage
                {
                    Role = ChatMessageRole.Tool,
                    Content = ExtractContentText(toolChatMessage.Content),
                    ToolCallId = toolChatMessage.ToolCallId
                },
                _ => null
            };

            if (cachedMessage != null)
            {
                result.Add(cachedMessage);
            }
            else
            {
                Console.WriteLine($"Unknown message type: {message.GetType()}");
            }
        }

        return result;
    }

    private static List<ChatMessage> ToChatMessages(IEnumerable<CachedChatMessage> messages)
    {
        var result = new List<ChatMessage>();

        foreach (var message in messages)
        {
            ChatMessage? chatMessage = message.Role switch
            {
                ChatMessageRole.User => ChatMessage.CreateUserMessage(message.Content),
                ChatMessageRole.Assistant => ChatMessage.CreateAssistantMessage(message.Content),
                ChatMessageRole.System => ChatMessage.CreateSystemMessage(message.Content),
                ChatMessageRole.Tool => ChatMessage.CreateToolMessage(message.Content, message.ToolCallId),
                _ => null
            };

            if (chatMessage != null)
            {
                result.Add(chatMessage);
            }
            else
            {
                Console.WriteLine($"Unknown message type: {message.Role}");
            }
        }

        return result;
    }

    private static string ExtractContentText(ChatMessageContent content)
    {
        return content
            .First(x => x.Kind == ChatMessageContentPartKind.Text)
            .Text ?? string.Empty;
    }
}

