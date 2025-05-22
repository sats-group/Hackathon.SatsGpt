namespace SATS.AI.Models;

public record DocumentDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public required string Path { get; set; }
}
