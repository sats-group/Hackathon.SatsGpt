using OpenAI.Chat;

namespace SATS.AI.Chat;

public class CachedChatMessage
{
    public required ChatMessageRole Role { get; set; }
    public required string Content { get; set; }
    public string? ToolCallId { get; set; }    
}

public class CachedChatToolCall
{
    public required string FunctionName { get; set; }
    public required string FunctionArguments { get; set; }
}
