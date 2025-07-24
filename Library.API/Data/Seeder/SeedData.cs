using Bogus;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace Library.API.Data.Seeder
{
    public static class SeedData
    {
        public static void Initialize(LibraryDbContext context)
        {
            if (context.Books.AsNoTracking().Any())
                return;

            using var transaction = context.Database.BeginTransaction();  // <-- เพิ่มตรงนี้

            var rnd = new Random();

            var authorFaker = new Faker<Author>()
                .RuleFor(a => a.FirstName, f => f.Name.FirstName())
                .RuleFor(a => a.LastName, f => f.Name.LastName())
                .RuleFor(a => a.PenName, f => f.Internet.UserName());

            var authors = authorFaker.Generate(100_000);

            context.BulkInsert(authors, new BulkConfig
            {
                SetOutputIdentity = true,
                BatchSize = 10000,
                UseTempDB = true,
                BulkCopyTimeout = 600,
            });

            var bookFaker = new Faker<Book>()
                .RuleFor(b => b.Title, f => f.Lorem.Sentence(3))
                .RuleFor(b => b.Publisher, f => f.Company.CompanyName())
                .RuleFor(b => b.Price, f => f.Random.Decimal(10, 100));

            var books = bookFaker.Generate(150_000);

            context.BulkInsert(books, new BulkConfig
            {
                SetOutputIdentity = true,
                BatchSize = 10000,
                UseTempDB = true,
                BulkCopyTimeout = 600,
            });

            List<T> GetRandomSubset<T>(List<T> list, int count, Random random)
            {
                var copy = list.ToArray();
                for (int i = 0; i < count; i++)
                {
                    int swapIndex = i + random.Next(copy.Length - i);
                    var temp = copy[i];
                    copy[i] = copy[swapIndex];
                    copy[swapIndex] = temp;
                }
                return copy.Take(count).ToList();
            }

            var bookAuthors = new List<BookAuthor>(books.Count * 2);

            for (int i = 0; i < books.Count; i++)
            {
                int authorCount = rnd.Next(1, 4);

                var selectedAuthors = GetRandomSubset(authors, authorCount, rnd);

                for (int j = 0; j < selectedAuthors.Count; j++)
                {
                    bookAuthors.Add(new BookAuthor
                    {
                        BookId = books[i].BookId,
                        AuthorId = selectedAuthors[j].AuthorId
                    });
                }
            }

            int batchSize = 10000;
            for (int i = 0; i < bookAuthors.Count; i += batchSize)
            {
                var batch = bookAuthors.Skip(i).Take(batchSize).ToList();
                context.BulkInsert(batch, new BulkConfig
                {
                    BatchSize = batchSize,
                    UseTempDB = true,
                    BulkCopyTimeout = 600,
                });
            }

            transaction.Commit();  
        }

    }
}
