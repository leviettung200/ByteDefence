using BookStore.Api.Data;
using BookStore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Api.GraphQL.Types;

[ExtendObjectType<Author>]
public class AuthorTypeExtension
{
    /// <summary>
    /// Custom resolver: Gets the total number of books by this author.
    /// </summary>
    public async Task<int> GetBookCount(
        [Parent] Author author,
        [Service] BookStoreDbContext context)
    {
        return await context.Books
            .Where(b => b.AuthorId == author.Id)
            .CountAsync();
    }
}
