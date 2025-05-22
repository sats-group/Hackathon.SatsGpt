using OpenAI.Chat;

namespace SATS.AI.Chat;

public static class ChatMessageMapper
{
    public static List<CachedChatMessage> MapCachedChatMessages(List<ChatMessage> messages)
    {
        var result = new List<CachedChatMessage>();

        foreach (var message in messages)
        {
            var id = Guid.NewGuid().ToString();
            var cachedMessage = message switch
            {
                UserChatMessage userMessage => new CachedChatMessage
                {
                    Role = ChatMessageRole.User,
                    Content = ExtractContentText(userMessage.Content),
                    ToolCallId = null,
                    Id = id
                },
                AssistantChatMessage assistantChatMessage => new CachedChatMessage
                {
                    Role = ChatMessageRole.Assistant,
                    Content = ExtractContentText(assistantChatMessage.Content),
                    ToolCallId = assistantChatMessage.ToolCalls.FirstOrDefault()?.Id,
                    Id = id
                },
                SystemChatMessage systemChatMessage => new CachedChatMessage
                {
                    Role = ChatMessageRole.System,
                    Content = ExtractContentText(systemChatMessage.Content),
                    ToolCallId = null,
                    Id = id
                },
                ToolChatMessage toolChatMessage => new CachedChatMessage
                {
                    Role = ChatMessageRole.Tool,
                    Content = ExtractContentText(toolChatMessage.Content),
                    ToolCallId = toolChatMessage.ToolCallId,
                    Id = id
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

    public static List<ChatMessage> MapChatMessages(IEnumerable<CachedChatMessage> messages)
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
