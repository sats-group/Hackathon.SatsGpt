using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;
using SATS.AI.Chat;
using SATS.AI.Runners;

namespace SATS.AI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController(AgentRunner runner, ChatStore store) : ControllerBase
{
    [HttpGet]
    public IActionResult GetChatList()
    {
        var chats = store.GetAll();
        return Ok(chats);
    }

    [HttpGet("{chatId}")]
    public IActionResult GetChat([FromRoute] string chatId)
    {
        var chat = store.Get(chatId);
        if (chat == null)
        {
            return NotFound();
        }

        return Ok(chat);
    }

    [HttpPost("{chatId}")]
    public async Task StreamChat([FromRoute] string chatId, [FromBody] string message)
    {
        Response.ContentType = "text/plain";
        Response.Headers.CacheControl = "no-cache";

        var streamWriter = new StreamWriter(Response.BodyWriter.AsStream());
        var cancellationToken = HttpContext.RequestAborted;

        CachedChat chat;

        if (store.Contains(chatId))
        {
            chat = store.Get(chatId)!;
            
            chat.Messages.Add(new CachedChatMessage
            {
                Role = ChatMessageRole.User,
                Content = message
            });
        }
        else
        {
            chat = new CachedChat
            {
                Id = chatId,
                Name = "Untitled",
                CreatedAt = DateTime.UtcNow,
                Messages =
                [
                    new ()
                    {
                        Role = ChatMessageRole.System,
                        Content = @"
                            You are a helpful assistant. You will answer questions with a focus on clarity and brevity. Try and keep
                            responses to a maximum of 3 sentences. If you don't know the answer, say 'I don't know'.
                        "
                    },
                    new ()
                    {
                        Role = ChatMessageRole.User,
                        Content = message
                    }
                ]
            };
        }

        var chatMessages = ChatMessageMapper.MapChatMessages(chat.Messages);

        var updates = runner.RunStreamAsync(chatMessages);

        await foreach (var content in updates)
        {
            await streamWriter.WriteLineAsync(content);
            await streamWriter.FlushAsync(cancellationToken);
        }

        chat.Messages = ChatMessageMapper.MapCachedChatMessages(chatMessages);
        store.Set(chatId, chat);
    }
}
