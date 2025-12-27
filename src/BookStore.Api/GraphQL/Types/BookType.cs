using BookStore.Api.Data;
using BookStore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Api.GraphQL.Types;

[ExtendObjectType<Book>]
public class BookTypeExtension
{
    /// <summary>
    /// Custom resolver: Gets the average rating for a book.
    /// </summary>
    public async Task<double?> GetAverageRating(
        [Parent] Book book,
        [Service] BookStoreDbContext context)
    {
        var reviews = await context.Reviews
            .Where(r => r.BookId == book.Id)
            .AsNoTracking()
            .ToListAsync();

        if (reviews.Count == 0) return null;
        return reviews.Average(r => r.Rating);
    }

    /// <summary>
    /// Custom resolver: Gets the total review count for a book.
    /// </summary>
    public async Task<int> GetReviewCount(
        [Parent] Book book,
        [Service] BookStoreDbContext context)
    {
        return await context.Reviews
            .Where(r => r.BookId == book.Id)
            .CountAsync();
    }
}
