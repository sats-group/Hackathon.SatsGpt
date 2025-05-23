using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;
using OpenAI.Chat;
using SATS.AI.Schema;
using SATS.AI.Tools;

namespace SATS.AI.Runners;

public class AgentRunner(ChatClient chatClient, IEnumerable<ITool> tools) 
{
    public async Task<T> RunAsync<T>(
        ICollection<ChatMessage> chatMessages,        
        CancellationToken cancellationToken = default)
    {
        var options = BuildChatCompletionOptions<T>(tools);

        bool requiresToolExecution;
        do
        {
            requiresToolExecution = false;
            var completion = await chatClient.CompleteChatAsync(chatMessages, options, cancellationToken);

            switch (completion.Value.FinishReason)
            {
                case ChatFinishReason.Stop:
                    chatMessages.Add(ChatMessage.CreateAssistantMessage(completion.Value));
                    break;
                case ChatFinishReason.ToolCalls:
                    chatMessages.Add(ChatMessage.CreateAssistantMessage(completion.Value));
                    var toolCalls = completion.Value.ToolCalls;
                    await ProcessToolCallsAsync(toolCalls, tools, chatMessages, cancellationToken);
                    requiresToolExecution = true;
                    break;
                default:
                    throw new Exception("Unexpected finish reason");
            }
        } while (requiresToolExecution);

        var json = ExtractResponseContent(chatMessages);
        var result = JsonConvert.DeserializeObject<T>(json);
        
        return result!;
    }

    public async IAsyncEnumerable<string> RunStreamAsync(
        ICollection<ChatMessage> chatMessages,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var options = BuildChatCompletionOptions<string>(tools);

        bool requiresToolExecution;
        do
        {
            requiresToolExecution = false;
            var toolCallsBuilder = new StreamingChatToolCallsBuilder();
            var contentBuilder = new StringBuilder();

            await foreach (StreamingChatCompletionUpdate update in chatClient.CompleteChatStreamingAsync(chatMessages, options, cancellationToken))
            {
                foreach (ChatMessageContentPart contentPart in update.ContentUpdate)
                {
                    yield return contentPart.Text;
                    contentBuilder.Append(contentPart.Text);
                }

                foreach (StreamingChatToolCallUpdate toolCallUpdate in update.ToolCallUpdates)
                {
                    toolCallsBuilder.Append(toolCallUpdate);
                }

                switch (update.FinishReason)
                {
                    case ChatFinishReason.Stop:
                        chatMessages.Add(new AssistantChatMessage(contentBuilder.ToString()));
                        break;
                    case ChatFinishReason.ToolCalls:
                        var toolCalls = toolCallsBuilder.Build();
                        var assistantMessage = BuildAssistantMessage(contentBuilder, toolCalls);
                        chatMessages.Add(assistantMessage);

                        foreach (var toolCall in toolCalls)
                        {
                            var tool = tools.FirstOrDefault(t => t.Name == toolCall.FunctionName)
                                ?? throw new InvalidOperationException($"Tool '{toolCall.FunctionName}' not found.");

                            var toolResult = await tool.ExecuteAsync(toolCall.FunctionArguments.ToString(), cancellationToken);
                            var toolResultJson = JsonConvert.SerializeObject(toolResult);
                            chatMessages.Add(new ToolChatMessage(toolCall.Id, toolResultJson));
                        }

                        requiresToolExecution = true;
                        break;
                    case ChatFinishReason.Length:
                        throw new Exception("Incomplete model output due to MaxTokens parameter or token limit exceeded.");
                    case ChatFinishReason.ContentFilter:
                        throw new Exception("Omitted content due to a content filter flag.");
                    default:
                        if (update.FinishReason != null)
                        {
                            throw new Exception("Unexpected finish reason");
                        }
                        break;
                }
            }
        } while (requiresToolExecution);

        System.Console.WriteLine("Stream completed.");
    }

    private static ChatCompletionOptions BuildChatCompletionOptions<T>(IEnumerable<ITool> tools)
    {
        var options = new ChatCompletionOptions();

        foreach (var tool in tools)
        {
            options.Tools.Add(tool.ToChatTool());
        }

        options.ResponseFormat = typeof(T) == typeof(string)
            ? ChatResponseFormat.CreateTextFormat()
            : ChatResponseFormat.CreateJsonSchemaFormat(
                typeof(T).Name,
                SchemaProvider.GenerateBinarySchema<T>(false),
                jsonSchemaIsStrict: true
            );

        return options;
    }

    private static AssistantChatMessage BuildAssistantMessage(StringBuilder contentBuilder, IEnumerable<ChatToolCall> toolCalls)
    {
        var assistantMessage = new AssistantChatMessage(toolCalls);
        if (contentBuilder.Length > 0)
        {
            assistantMessage.Content.Add(ChatMessageContentPart.CreateTextPart(contentBuilder.ToString()));
        }
        return assistantMessage;
    }

    private static async Task ProcessToolCallsAsync(
        IEnumerable<ChatToolCall> toolCalls,
        IEnumerable<ITool> tools,
        ICollection<ChatMessage> messages,
        CancellationToken cancellationToken)
    {
        foreach (var toolCall in toolCalls)
        {
            var tool = tools.FirstOrDefault(t => t.Name == toolCall.FunctionName);
            if (tool == null)
            {
                continue;
            }

            var toolResponse = await tool.ExecuteAsync(toolCall.FunctionArguments.ToString(), cancellationToken);
            string toolResultJson = JsonConvert.SerializeObject(toolResponse);
            messages.Add(ChatMessage.CreateToolMessage(toolCall.Id, toolResultJson));
        }
    }

    private static string ExtractResponseContent(IEnumerable<ChatMessage> messages)
    {
        var lastMessage = messages.LastOrDefault();
        if (lastMessage is not AssistantChatMessage assistantMessage)
        {
            throw new InvalidOperationException("The last message in the chat is not an assistant message.");
        }
        return assistantMessage.Content
            .Where(cp => cp.Kind == ChatMessageContentPartKind.Text)
            .Select(cp => cp.Text)
            .FirstOrDefault() ?? string.Empty;
    }
}
