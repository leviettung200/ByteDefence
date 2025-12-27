using BookStore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Api.Data;

public class BookStoreDbContext : DbContext
{
    public BookStoreDbContext(DbContextOptions<BookStoreDbContext> options) : base(options)
    {
    }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>()
            .HasOne(b => b.Author)
            .WithMany(a => a.Books)
            .HasForeignKey(b => b.AuthorId);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.Book)
            .WithMany(b => b.Reviews)
            .HasForeignKey(r => r.BookId);

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var author1Id = "author-1";
        var author2Id = "author-2";
        var author3Id = "author-3";

        modelBuilder.Entity<Author>().HasData(
            new Author
            {
                Id = author1Id,
                Name = "George Orwell",
                Biography = "English novelist and essayist, known for his critical commentary on political systems.",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Author
            {
                Id = author2Id,
                Name = "Jane Austen",
                Biography = "English novelist known for her romance novels set among the landed gentry.",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Author
            {
                Id = author3Id,
                Name = "Isaac Asimov",
                Biography = "American writer and professor of biochemistry, best known for science fiction works.",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );

        var book1Id = "book-1";
        var book2Id = "book-2";
        var book3Id = "book-3";
        var book4Id = "book-4";

        modelBuilder.Entity<Book>().HasData(
            new Book
            {
                Id = book1Id,
                Title = "1984",
                Description = "A dystopian novel set in Airstrip One, a province of the superstate Oceania.",
                Isbn = "978-0451524935",
                PublishedYear = 1949,
                Status = BookStatus.Published,
                AuthorId = author1Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Book
            {
                Id = book2Id,
                Title = "Animal Farm",
                Description = "An allegorical novella reflecting events leading up to the Russian Revolution.",
                Isbn = "978-0451526342",
                PublishedYear = 1945,
                Status = BookStatus.Published,
                AuthorId = author1Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Book
            {
                Id = book3Id,
                Title = "Pride and Prejudice",
                Description = "A romantic novel that charts the emotional development of protagonist Elizabeth Bennet.",
                Isbn = "978-0141439518",
                PublishedYear = 1813,
                Status = BookStatus.Published,
                AuthorId = author2Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Book
            {
                Id = book4Id,
                Title = "Foundation",
                Description = "The story of a mathematician who plans to preserve knowledge during a galactic dark age.",
                Isbn = "978-0553293357",
                PublishedYear = 1951,
                Status = BookStatus.Published,
                AuthorId = author3Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );

        modelBuilder.Entity<Review>().HasData(
            new Review
            {
                Id = "review-1",
                Title = "A masterpiece of dystopian fiction",
                Content = "Orwell's vision of a totalitarian future is chillingly prescient. A must-read.",
                Rating = 5,
                ReviewerName = "BookLover42",
                BookId = book1Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Review
            {
                Id = "review-2",
                Title = "Thought-provoking",
                Content = "Makes you question the nature of truth and freedom in modern society.",
                Rating = 4,
                ReviewerName = "CriticalReader",
                BookId = book1Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Review
            {
                Id = "review-3",
                Title = "Brilliant allegory",
                Content = "Simple on the surface but deeply meaningful. The animals bring history to life.",
                Rating = 5,
                ReviewerName = "HistoryBuff",
                BookId = book2Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Review
            {
                Id = "review-4",
                Title = "Timeless romance",
                Content = "Elizabeth Bennet remains one of literature's greatest heroines.",
                Rating = 5,
                ReviewerName = "RomanceReader",
                BookId = book3Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Review
            {
                Id = "review-5",
                Title = "Epic sci-fi",
                Content = "The scope of Asimov's imagination is breathtaking. Foundation laid the groundwork for modern sci-fi.",
                Rating = 5,
                ReviewerName = "SciFiFan",
                BookId = book4Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );
    }
}
