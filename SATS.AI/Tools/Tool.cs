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

public abstract class Tool : ITool
{
    public abstract string Name { get; }
    public abstract string Description { get; }

    public abstract Task<object?> ExecuteAsync(string jsonParameters, CancellationToken cancellationToken);

    /// <summary>
    /// Converts the tool into a ChatTool for use with an LLM.
    /// </summary>
    public ChatTool ToChatTool()
    {
        var jsonParameters = JsonConvert.SerializeObject(GetParameterSchema());
        var binaryParameters = BinaryData.FromString(jsonParameters);

        return ChatTool.CreateFunctionTool(Name, Description, binaryParameters);
    }

    protected abstract object GetParameterSchema();
}

public abstract class GenericTool<TInput, TOutput> : Tool
{
    public override async Task<object?> ExecuteAsync(string jsonParameters, CancellationToken cancellationToken)
    {
        if (typeof(TInput) == typeof(string))
        {
            return await ExecuteAsync((TInput)(object)jsonParameters, cancellationToken);
        }
        
        var input = JsonConvert.DeserializeObject<TInput>(jsonParameters);
        return await ExecuteAsync(input!, cancellationToken);
    }

    public abstract Task<TOutput?> ExecuteAsync(TInput input, CancellationToken cancellationToken);

    protected override object GetParameterSchema()
    {
        var methodInfo = GetType().GetMethod(nameof(ExecuteAsync), [typeof(TInput), typeof(CancellationToken)]);
        return SchemaProvider.GenerateParameterSchema(methodInfo!)!;
    }
}