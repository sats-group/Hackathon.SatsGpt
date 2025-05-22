using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenAI.Embeddings;
using Pgvector;
using SATS.AI.Documents;
using SATS.AI.Documents.Entities;
using SATS.AI.Extensions;

namespace SATS.AI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController(DocumentDbContext dbContext, EmbeddingClient embeddingClient) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetDocumentList()
    {
        var documents = await dbContext.Documents.ToListAsync();
        return Ok(documents);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDocument([FromRoute] Guid id)
    {
        var document = await dbContext.Documents.FindAsync(id);
        if (document == null)
        {
            return NotFound();
        }

        return Ok(new { document.Id, document.Title, document.Content, document.Path });
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchDocument([FromQuery] string query, [FromQuery] int limit = 5)
    {
        var embedding = await embeddingClient.GenerateEmbeddingAsync(query);
        var documents = await dbContext.Documents
            .SearchByEmbedding(embedding.Value.ToFloats().ToArray(), limit)
            .ToListAsync();

        var slimDocuments = documents.Select(d => new
        {
            d.Id,
            d.Title,
            d.Content,
            d.Path
        }).ToList();

        return Ok(slimDocuments);
    }

    [HttpGet("directory-search")]
    public async Task<IActionResult> GetDocumentUnderPath([FromQuery] string path)
    {
        var documents = await dbContext.Documents.GetDocumentsUnderPath(path).ToListAsync();

        var slimDocuments = documents.Select(d => new
        {
            d.Id,
            d.Title,
            d.Content,
            d.Path
        }).ToList();

        return Ok(documents);
    }

    [HttpPost]
    public async Task<IActionResult> CreateDocument([FromBody] CreateDocumentRequest request)
    {
        var embedding = await embeddingClient.GenerateEmbeddingAsync(request.Content);

        var document = new Document
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Content = request.Content,
            Path = request.Path,
            Embedding = new Vector(embedding.Value.ToFloats())
        };

        await dbContext.Documents.AddAsync(document);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(CreateDocument), new { id = document.Id }, document);
    }
}

public record CreateDocumentRequest(string Title, string Content, string Path);