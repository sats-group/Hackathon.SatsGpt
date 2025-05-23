using Newtonsoft.Json;
using OpenAI.Chat;
using SATS.AI.Schema;

namespace SATS.AI.Tools;

public interface ITool
{
    string Name { get; }
    string Description { get; }
    Task<object?> ExecuteAsync(string jsonParameters, CancellationToken cancellationToken);
    ChatTool ToChatTool();
}

public abstract class Tool<TInput, TOutput> : ITool
{
    public abstract string Name { get; }
    public abstract string Description { get; }

    public abstract Task<TOutput?> ExecuteAsync(TInput input, CancellationToken cancellationToken);

    public async Task<object?> ExecuteAsync(string jsonParameters, CancellationToken cancellationToken)
    {
        if (typeof(TInput) == typeof(string))
        {
            return await ExecuteAsync((TInput)(object)jsonParameters, cancellationToken);
        }

        var input = JsonConvert.DeserializeObject<TInput>(jsonParameters);
        return await ExecuteAsync(input!, cancellationToken);
    }

    /// <summary>
    /// Converts the tool into a ChatTool for use with an LLM.
    /// </summary>
    public ChatTool ToChatTool()
    {
        BinaryData? binaryParameters = null;

        var parameterSchema = GetParameterSchema();

        if (parameterSchema is not null)
        {
            var jsonSchema = JsonConvert.SerializeObject(parameterSchema);
            binaryParameters = BinaryData.FromString(jsonSchema);
        }

        return ChatTool.CreateFunctionTool(Name, Description, binaryParameters);
    }

    protected abstract object? GetParameterSchema();
}