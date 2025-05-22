using OpenAI.Chat;
using System.Text.Json;
using System.Text.Json.Serialization;

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
    [JsonConverter(typeof(CamelCaseEnumConverter))]
    public required ChatMessageRole Role { get; set; }
    public required string Content { get; set; }
    public string? ToolCallId { get; set; }
    public required string Id { get; set; }
}

public class CachedChatToolCall
{
    public required string FunctionName { get; set; }
    public required string FunctionArguments { get; set; }
}

public class CamelCaseEnumConverter : JsonConverter<ChatMessageRole>
{
    public override ChatMessageRole Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return Enum.Parse<ChatMessageRole>(value!, true);
    }

    public override void Write(Utf8JsonWriter writer, ChatMessageRole value, JsonSerializerOptions options)
    {
        var str = value.ToString();
        var camelCase = char.ToLowerInvariant(str[0]) + str[1..];
        writer.WriteStringValue(camelCase);
    }
}
