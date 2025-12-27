using BookStore.Api.Data;
using BookStore.Api.Models;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Api.GraphQL.Queries;

[QueryType]
public class Query
{
    /// <summary>
    /// Gets all books with optional filtering, sorting, and pagination.
    /// </summary>
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Book> GetBooks([Service] BookStoreDbContext context)
    {
        return context.Books.AsNoTracking();
    }

    /// <summary>
    /// Gets a book by its unique identifier.
    /// </summary>
    [UseProjection]
    public async Task<Book?> GetBookById(
        [Service] BookStoreDbContext context,
        string id)
    {
        return await context.Books
            .Include(b => b.Author)
            .Include(b => b.Reviews)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    /// <summary>
    /// Gets all authors.
    /// </summary>
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Author> GetAuthors([Service] BookStoreDbContext context)
    {
        return context.Authors.AsNoTracking();
    }

    /// <summary>
    /// Gets an author by their unique identifier.
    /// </summary>
    [UseProjection]
    public async Task<Author?> GetAuthorById(
        [Service] BookStoreDbContext context,
        string id)
    {
        return await context.Authors
            .Include(a => a.Books)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    /// <summary>
    /// Gets all reviews.
    /// </summary>
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Review> GetReviews([Service] BookStoreDbContext context)
    {
        return context.Reviews.AsNoTracking();
    }

    /// <summary>
    /// Demonstrates concurrent database queries by fetching books and authors in parallel.
    /// </summary>
    public async Task<ConcurrentQueryResult> GetConcurrentData([Service] BookStoreDbContext context)
    {
        var booksTask = context.Books.AsNoTracking().ToListAsync();
        var authorsTask = context.Authors.AsNoTracking().ToListAsync();
        var reviewsTask = context.Reviews.AsNoTracking().ToListAsync();

        await Task.WhenAll(booksTask, authorsTask, reviewsTask);

        return new ConcurrentQueryResult
        {
            Books = await booksTask,
            Authors = await authorsTask,
            Reviews = await reviewsTask
        };
    }

    /// <summary>
    /// Demonstrates error handling - throws an error when simulate is true.
    /// </summary>
    public Book GetBookWithError(bool simulateError)
    {
        if (simulateError)
        {
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage("Simulated error for testing purposes")
                    .SetCode("SIMULATED_ERROR")
                    .Build());
        }

        return new Book
        {
            Id = "mock-book",
            Title = "Mock Book",
            Description = "This is a mock book for testing",
            Status = BookStatus.Draft
        };
    }
}

public class ConcurrentQueryResult
{
    public List<Book> Books { get; set; } = new();
    public List<Author> Authors { get; set; } = new();
    public List<Review> Reviews { get; set; } = new();
}
