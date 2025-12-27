using BookStore.Api.Data;
using BookStore.Api.GraphQL.Subscriptions;
using BookStore.Api.Models;
using HotChocolate.Authorization;
using HotChocolate.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Api.GraphQL.Mutations;

[MutationType]
public class Mutation
{
    /// <summary>
    /// Creates a new book. Requires authentication.
    /// </summary>
    [Authorize]
    public async Task<BookPayload> CreateBook(
        CreateBookInput input,
        [Service] BookStoreDbContext context,
        [Service] ITopicEventSender eventSender)
    {
        var author = await context.Authors.FindAsync(input.AuthorId);
        if (author == null)
        {
            return new BookPayload(null, new[] { new UserError("Author not found", "AUTHOR_NOT_FOUND") });
        }

        var book = new Book
        {
            Id = Guid.NewGuid().ToString(),
            Title = input.Title,
            Description = input.Description,
            Isbn = input.Isbn,
            PublishedYear = input.PublishedYear,
            Status = input.Status ?? BookStatus.Draft,
            AuthorId = input.AuthorId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Books.Add(book);
        await context.SaveChangesAsync();

        // Load the author for the response
        book.Author = author;

        await eventSender.SendAsync(nameof(Subscription.OnBookCreated), book);

        return new BookPayload(book);
    }

    /// <summary>
    /// Updates an existing book. Requires authentication.
    /// </summary>
    [Authorize]
    public async Task<BookPayload> UpdateBook(
        UpdateBookInput input,
        [Service] BookStoreDbContext context,
        [Service] ITopicEventSender eventSender)
    {
        var book = await context.Books
            .Include(b => b.Author)
            .FirstOrDefaultAsync(b => b.Id == input.Id);

        if (book == null)
        {
            return new BookPayload(null, new[] { new UserError("Book not found", "BOOK_NOT_FOUND") });
        }

        if (input.Title != null) book.Title = input.Title;
        if (input.Description != null) book.Description = input.Description;
        if (input.Isbn != null) book.Isbn = input.Isbn;
        if (input.PublishedYear.HasValue) book.PublishedYear = input.PublishedYear.Value;
        if (input.Status.HasValue) book.Status = input.Status.Value;
        
        if (input.AuthorId != null && input.AuthorId != book.AuthorId)
        {
            var author = await context.Authors.FindAsync(input.AuthorId);
            if (author == null)
            {
                return new BookPayload(null, new[] { new UserError("Author not found", "AUTHOR_NOT_FOUND") });
            }
            book.AuthorId = input.AuthorId;
            book.Author = author;
        }

        book.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        await eventSender.SendAsync(nameof(Subscription.OnBookUpdated), book);

        return new BookPayload(book);
    }

    /// <summary>
    /// Deletes a book. Requires authentication.
    /// </summary>
    [Authorize]
    public async Task<DeletePayload> DeleteBook(
        string id,
        [Service] BookStoreDbContext context,
        [Service] ITopicEventSender eventSender)
    {
        var book = await context.Books.FindAsync(id);
        if (book == null)
        {
            return new DeletePayload(false, new[] { new UserError("Book not found", "BOOK_NOT_FOUND") });
        }

        // Delete associated reviews first
        var reviews = await context.Reviews.Where(r => r.BookId == id).ToListAsync();
        context.Reviews.RemoveRange(reviews);
        
        context.Books.Remove(book);
        await context.SaveChangesAsync();

        await eventSender.SendAsync(nameof(Subscription.OnBookDeleted), id);

        return new DeletePayload(true);
    }

    /// <summary>
    /// Creates a new author. Requires authentication.
    /// </summary>
    [Authorize]
    public async Task<AuthorPayload> CreateAuthor(
        CreateAuthorInput input,
        [Service] BookStoreDbContext context)
    {
        var author = new Author
        {
            Id = Guid.NewGuid().ToString(),
            Name = input.Name,
            Biography = input.Biography,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Authors.Add(author);
        await context.SaveChangesAsync();

        return new AuthorPayload(author);
    }

    /// <summary>
    /// Creates a new review for a book. Requires authentication.
    /// </summary>
    [Authorize]
    public async Task<ReviewPayload> CreateReview(
        CreateReviewInput input,
        [Service] BookStoreDbContext context,
        [Service] ITopicEventSender eventSender)
    {
        var book = await context.Books.FindAsync(input.BookId);
        if (book == null)
        {
            return new ReviewPayload(null, new[] { new UserError("Book not found", "BOOK_NOT_FOUND") });
        }

        if (input.Rating < 1 || input.Rating > 5)
        {
            return new ReviewPayload(null, new[] { new UserError("Rating must be between 1 and 5", "INVALID_RATING") });
        }

        var review = new Review
        {
            Id = Guid.NewGuid().ToString(),
            Title = input.Title,
            Content = input.Content,
            Rating = input.Rating,
            ReviewerName = input.ReviewerName,
            BookId = input.BookId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Reviews.Add(review);
        await context.SaveChangesAsync();

        await eventSender.SendAsync(nameof(Subscription.OnReviewAdded), review);

        return new ReviewPayload(review);
    }
}

// Input types
public record CreateBookInput(
    string Title,
    string? Description,
    string? Isbn,
    int PublishedYear,
    string AuthorId,
    BookStatus? Status);

public record UpdateBookInput(
    string Id,
    string? Title,
    string? Description,
    string? Isbn,
    int? PublishedYear,
    string? AuthorId,
    BookStatus? Status);

public record CreateAuthorInput(
    string Name,
    string? Biography);

public record CreateReviewInput(
    string BookId,
    string Title,
    string? Content,
    int Rating,
    string ReviewerName);

// Payload types
public class BookPayload
{
    public Book? Book { get; }
    public IReadOnlyList<UserError> Errors { get; }

    public BookPayload(Book? book, IReadOnlyList<UserError>? errors = null)
    {
        Book = book;
        Errors = errors ?? Array.Empty<UserError>();
    }
}

public class AuthorPayload
{
    public Author? Author { get; }
    public IReadOnlyList<UserError> Errors { get; }

    public AuthorPayload(Author? author, IReadOnlyList<UserError>? errors = null)
    {
        Author = author;
        Errors = errors ?? Array.Empty<UserError>();
    }
}

public class ReviewPayload
{
    public Review? Review { get; }
    public IReadOnlyList<UserError> Errors { get; }

    public ReviewPayload(Review? review, IReadOnlyList<UserError>? errors = null)
    {
        Review = review;
        Errors = errors ?? Array.Empty<UserError>();
    }
}

public class DeletePayload
{
    public bool Success { get; }
    public IReadOnlyList<UserError> Errors { get; }

    public DeletePayload(bool success, IReadOnlyList<UserError>? errors = null)
    {
        Success = success;
        Errors = errors ?? Array.Empty<UserError>();
    }
}

public record UserError(string Message, string Code);
