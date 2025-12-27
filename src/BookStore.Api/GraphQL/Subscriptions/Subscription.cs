using BookStore.Api.Models;

namespace BookStore.Api.GraphQL.Subscriptions;

[SubscriptionType]
public class Subscription
{
    /// <summary>
    /// Subscribes to new book creation events.
    /// </summary>
    [Subscribe]
    [Topic]
    public Book OnBookCreated([EventMessage] Book book) => book;

    /// <summary>
    /// Subscribes to book update events.
    /// </summary>
    [Subscribe]
    [Topic]
    public Book OnBookUpdated([EventMessage] Book book) => book;

    /// <summary>
    /// Subscribes to book deletion events.
    /// </summary>
    [Subscribe]
    [Topic]
    public string OnBookDeleted([EventMessage] string bookId) => bookId;

    /// <summary>
    /// Subscribes to new review events.
    /// </summary>
    [Subscribe]
    [Topic]
    public Review OnReviewAdded([EventMessage] Review review) => review;
}
