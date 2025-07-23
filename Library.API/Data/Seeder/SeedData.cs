using Bogus;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Data.Seeder
{
    public static class SeedData
    {
        public static async Task Initialize(LibraryDbContext context)
        {
            if (await context.Books.AnyAsync())
                return;

            var rnd = new Random();

            var authorFaker = new Faker<Author>()
                .RuleFor(a => a.FirstName, f => f.Name.FirstName())
                .RuleFor(a => a.LastName, f => f.Name.LastName())
                .RuleFor(a => a.PenName, f => f.Internet.UserName());

            var authors = authorFaker.Generate(1000);
            await context.Authors.AddRangeAsync(authors);
            await context.SaveChangesAsync();

            var bookFaker = new Faker<Book>()
                .RuleFor(b => b.Title, f => f.Lorem.Sentence(3))
                .RuleFor(b => b.Publisher, f => f.Company.CompanyName())
                .RuleFor(b => b.Price, f => f.Random.Decimal(10, 100));

            var books = bookFaker.Generate(10_000);
            await context.Books.AddRangeAsync(books);
            await context.SaveChangesAsync();

            var bookAuthors = new List<BookAuthor>();

            List<T> GetRandomSubset<T>(List<T> list, int count, Random random)
            {
                var result = new List<T>(count);
                var takenIndices = new HashSet<int>();

                while (result.Count < count)
                {
                    int index = random.Next(list.Count);
                    if (takenIndices.Add(index))
                    {
                        result.Add(list[index]);
                    }
                }

                return result;
            }

            foreach (var book in books)
            {
                int authorCount = rnd.Next(1, 4);
                var selectedAuthors = GetRandomSubset(authors, authorCount, rnd);

                foreach (var author in selectedAuthors)
                {
                    bookAuthors.Add(new BookAuthor
                    {
                        BookId = book.BookId,
                        AuthorId = author.AuthorId
                    });
                }
            }

            await context.BookAuthors.AddRangeAsync(bookAuthors);
            await context.SaveChangesAsync();
        }
    }
}
