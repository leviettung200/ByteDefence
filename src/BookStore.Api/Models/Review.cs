namespace BookStore.Api.Models;

public class Review
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public int Rating { get; set; } // 1-5 stars
    public string ReviewerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public string BookId { get; set; } = string.Empty;
    public virtual Book? Book { get; set; }
}
