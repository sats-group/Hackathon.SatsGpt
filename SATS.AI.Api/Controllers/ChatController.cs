using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;
using SATS.AI.Chat;
using SATS.AI.Runners;

namespace SATS.AI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController(AgentRunner runner, ChatStore store) : ControllerBase
{
    [HttpPost("{chatId}")]
    public async Task Stream([FromRoute] string chatId, [FromBody] string message)
    {
        Response.ContentType = "text/plain";
        Response.Headers.CacheControl = "no-cache";

        var streamWriter = new StreamWriter(Response.BodyWriter.AsStream());
        var cancellationToken = HttpContext.RequestAborted;

        var chatMessages = store.Get(chatId);

        chatMessages.Add(ChatMessage.CreateUserMessage(message));

        var updates = runner.RunStreamAsync(chatMessages);

        await foreach (var content in updates.WithCancellation(cancellationToken))
        {
            await streamWriter.WriteLineAsync(content);
            await streamWriter.FlushAsync(cancellationToken);
        }

        store.Set(chatId, chatMessages);
    }
}
