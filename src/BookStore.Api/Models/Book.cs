namespace BookStore.Api.Models;

public class Book
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Isbn { get; set; }
    public int PublishedYear { get; set; }
    public BookStatus Status { get; set; } = BookStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public string AuthorId { get; set; } = string.Empty;
    public virtual Author? Author { get; set; }
    
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}

public enum BookStatus
{
    Draft,
    Published,
    OutOfPrint,
    Archived
}
