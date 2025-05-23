using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;
using SATS.AI.Chat;
using SATS.AI.Runners;

namespace SATS.AI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController(AgentRunner runner, ChatStore store, ChatNameProvider nameProvider) : ControllerBase
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

        // Filter out system messages
        var filteredChat = new CachedChat
        {
            Id = chat.Id,
            Name = chat.Name,
            CreatedAt = chat.CreatedAt,
            Messages = chat.Messages.Where(m => m.Role != ChatMessageRole.System).ToList()
        };

        return Ok(filteredChat);
    }

    [HttpPost("{chatId}/summarize")]
    public async Task<IActionResult> SummarizeChat([FromRoute] string chatId)
    {
        var chat = store.Get(chatId);
        if (chat == null)
        {
            return NotFound();
        }

        var messageThread = chat.Messages.Select(m => m.Content).Take(5).ToArray();
        var summary = await nameProvider.GenerateChatName(string.Join("\n\n", messageThread));
        chat.Name = summary;
        store.Set(chatId, chat);
        return Ok(summary);
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
                Content = message,
                Id = Guid.NewGuid().ToString()
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
                    new () { Id = Guid.NewGuid().ToString(), Role = ChatMessageRole.System, Content = SystemPrompt.Default },
                    new () { Id = Guid.NewGuid().ToString(), Role = ChatMessageRole.User, Content = message }
                ]
            };
        }

        var chatMessages = ChatMessageMapper.MapChatMessages(chat.Messages);

        var updates = runner.RunStreamAsync(chatMessages);

        await foreach (var content in updates)
        {
            await streamWriter.WriteAsync(content);
            await streamWriter.FlushAsync(cancellationToken);
        }

        chat.Messages = ChatMessageMapper.MapCachedChatMessages(chatMessages);
        store.Set(chatId, chat);
    }
}
