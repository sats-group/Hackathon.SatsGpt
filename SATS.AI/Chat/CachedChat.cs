using OpenAI.Chat;

namespace SATS.AI.Chat;

public class CachedChat
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required List<CachedChatMessage> Messages { get; set; }
}

public class CachedChatMessage
{
    public required ChatMessageRole Role { get; set; }
    public required string Content { get; set; }
    public string? ToolCallId { get; set; }
    public List<CachedChatToolCall> ToolCalls { get; set; } = [];
}

public class CachedChatToolCall
{
    public required string Id { get; set; }
    public required string FunctionName { get; set; }
    public required string FunctionArguments { get; set; }
}
