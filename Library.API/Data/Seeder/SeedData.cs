using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Library.API.Data.Seeder
{

public static class SeedData
{
    public static void Initialize(LibraryDbContext context)
    {
        if (context.Books.Any())
            return;

        var rnd = new Random();

        // สร้าง 1000 authors
        var authorFaker = new Faker<Author>()
            .RuleFor(a => a.FirstName, f => f.Name.FirstName())
            .RuleFor(a => a.LastName, f => f.Name.LastName())
            .RuleFor(a => a.PenName, f => f.Internet.UserName());

        var authors = authorFaker.Generate(1000);
        context.Authors.AddRange(authors);
        context.SaveChanges();  // ต้อง Save เพื่อให้ AuthorId มีค่า

        // สร้าง 10,000 books
        var bookFaker = new Faker<Book>()
            .RuleFor(b => b.Title, f => f.Lorem.Sentence(3))
            .RuleFor(b => b.Publisher, f => f.Company.CompanyName())
            .RuleFor(b => b.Price, f => f.Random.Decimal(10, 100));

        var books = bookFaker.Generate(10_000);
        context.Books.AddRange(books);
        context.SaveChanges(); // Save เพื่อให้ BookId มีค่า

        // สร้างความสัมพันธ์แบบสุ่ม 1-3 authors ต่อหนังสือ 10,000 เล่ม
        var bookAuthors = new List<BookAuthor>();

        foreach (var book in books)
        {
            var authorCount = rnd.Next(1, 4);
            var selectedAuthors = authors.OrderBy(a => rnd.Next()).Take(authorCount);

            foreach (var author in selectedAuthors)
            {
                bookAuthors.Add(new BookAuthor { BookId = book.BookId, AuthorId = author.AuthorId });
            }
        }

        context.BookAuthors.AddRange(bookAuthors);
        context.SaveChanges();
    }
}
}