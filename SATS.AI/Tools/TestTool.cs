
namespace SATS.AI.Tools;

public class TestTool() : DynamicTool<string, string>("Test", "Just a simple test function", Foo)
{
    static Task<string?> Foo(string input, CancellationToken cancellationToken)
    {
        return Task.FromResult($"The secret is: Banana cake") as Task<string?>;
    }
}
