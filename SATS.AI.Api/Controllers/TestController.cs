using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;
using SATS.AI.Runners;

namespace SATS.AI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController(AgentRunner runner) : ControllerBase
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

        await foreach (var chatUpdate in updates.WithCancellation(cancellationToken))
        {
            await streamWriter.WriteLineAsync(chatUpdate.Content);
            await streamWriter.FlushAsync(cancellationToken);
        }
    }
}
