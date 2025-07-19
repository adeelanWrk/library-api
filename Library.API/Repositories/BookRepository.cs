using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class BookRepository : IBookRepository
{
    private readonly LibraryDbContext _db;
    public BookRepository(LibraryDbContext db) => _db = db;

    public async Task<List<BookDto>> GetBooksByAuthorId(int authorId)
    {
        return await _db.BookAuthors
            .Where(ba => ba.AuthorId == authorId)
            .Select(ba => new BookDto
            {
                BookId = ba.BookId,
                Title = ba.Book.Title,
                Publisher = ba.Book.Publisher
            }).ToListAsync();
    }

    public async Task<List<BookWithAuthorsDto>> GetBooksWithAuthorsPaged(int page, int pageSize, string sortBy, string sortDirection)
    {
        IQueryable<Book> query = _db.Books.Include(b => b.BookAuthors).ThenInclude(ba => ba.Author);

        // Apply sorting dynamically
        query = sortBy.ToLower() switch
        {
            "title" => sortDirection.ToLower() == "desc" ? query.OrderByDescending(b => b.Title) : query.OrderBy(b => b.Title),
            "publisher" => sortDirection.ToLower() == "desc" ? query.OrderByDescending(b => b.Publisher) : query.OrderBy(b => b.Publisher),
            "price" => sortDirection.ToLower() == "desc" ? query.OrderByDescending(b => b.Price) : query.OrderBy(b => b.Price),
            _ => query.OrderBy(b => b.BookId)
        };

        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BookWithAuthorsDto
            {
                Title = b.Title,
                Authors = b.BookAuthors.Select(ba => ba.Author.PenName ?? ba.Author.FirstName + " " + ba.Author.LastName).ToList(),
                AuthorCount = b.BookAuthors.Count
            }).ToListAsync();
    }
}
