using OpenAI.Chat;

namespace SATS.AI.Chat;

public static class ChatMessageMapper
{
    public static List<CachedChatMessage> MapCachedChatMessages(List<ChatMessage> messages)
    {
        var result = new List<CachedChatMessage>();

        foreach (var message in messages)
        {
            var cachedMessage = message switch
            {
                SystemChatMessage systemChatMessage => new CachedChatMessage
                {
                    Role = ChatMessageRole.System,
                    Content = ExtractContentText(systemChatMessage.Content),
                    ToolCallId = null
                },
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
                    ToolCalls = MapToolCalls(assistantChatMessage.ToolCalls),
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

    public static List<ChatMessage> MapChatMessages(IEnumerable<CachedChatMessage> messages)
    {
        var result = new List<ChatMessage>();

        foreach (var message in messages)
        {
            ChatMessage? chatMessage = message.Role switch
            {
                ChatMessageRole.User => ChatMessage.CreateUserMessage(message.Content),
                ChatMessageRole.Assistant => MapAssistantMessage(message),
                ChatMessageRole.System => ChatMessage.CreateSystemMessage(message.Content),
                ChatMessageRole.Tool => ChatMessage.CreateToolMessage(message.ToolCallId, message.Content),
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

    private static AssistantChatMessage MapAssistantMessage(CachedChatMessage message)
    {
        var assistant = ChatMessage.CreateAssistantMessage(message.Content);

        foreach (var toolCall in message.ToolCalls)
        {
            var tool = ChatToolCall.CreateFunctionToolCall(toolCall.Id, toolCall.FunctionName, BinaryData.FromString(toolCall.FunctionArguments));
            assistant.ToolCalls.Add(tool);
        }

        return assistant;
    }

    private static List<CachedChatToolCall> MapToolCalls(IEnumerable<ChatToolCall> toolCalls)
    {
        var result = new List<CachedChatToolCall>();

        foreach (var toolCall in toolCalls)
        {
            result.Add(new CachedChatToolCall
            {
                Id = toolCall.Id,
                FunctionName = toolCall.FunctionName,
                FunctionArguments = toolCall.FunctionArguments.ToString()
            });
        }

        return result;
    }

    private static string ExtractContentText(ChatMessageContent content)
    {
        return content
            .FirstOrDefault(x => x.Kind == ChatMessageContentPartKind.Text)
            ?.Text ?? string.Empty;
    }
}
