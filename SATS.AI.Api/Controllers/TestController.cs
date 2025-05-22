using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;
using SATS.AI.Chat;
using SATS.AI.Runners;

namespace SATS.AI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController(AgentRunner runner, ChatStore chatStore) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var chatMessages = new List<ChatMessage>
        {
            ChatMessage.CreateUserMessage("Hello, how are you?")
        };

        var response = await runner.RunAsync(chatMessages);
        return Ok(response);
    }

    [HttpGet("stream")]
    public async Task GetStream()
    {
        Response.ContentType = "text/plain";
        Response.Headers.CacheControl = "no-cache";

        var streamWriter = new StreamWriter(Response.BodyWriter.AsStream());
        var cancellationToken = HttpContext.RequestAborted;

        var chatMessages = new List<ChatMessage>
        {
            ChatMessage.CreateUserMessage("Do you have a tool named test? If so, can you use it to get the secret?")
        };

        var updates = runner.RunStreamAsync(chatMessages);

        await foreach (var content in updates.WithCancellation(cancellationToken))
        {
            await streamWriter.WriteLineAsync(content);
            await streamWriter.FlushAsync(cancellationToken);
        }
    }

    [HttpPost("stream")]
    public async Task GetStream2([FromQuery] string chatId, [FromBody] string message)
    {
        Response.ContentType = "text/plain";
        Response.Headers.CacheControl = "no-cache";

        var streamWriter = new StreamWriter(Response.BodyWriter.AsStream());
        var cancellationToken = HttpContext.RequestAborted;

        var chatMessages = chatStore.Get(chatId);

        chatMessages.Add(ChatMessage.CreateUserMessage(message));

        var updates = runner.RunStreamAsync(chatMessages);

        await foreach (var content in updates.WithCancellation(cancellationToken))
        {
            await streamWriter.WriteLineAsync(content);
            await streamWriter.FlushAsync(cancellationToken);
        }

        chatStore.Set(chatId, chatMessages);
    }
}
