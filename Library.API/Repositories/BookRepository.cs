using System.Collections.Generic;
using System.Threading.Tasks;
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

    public async Task<List<BookWithAuthorsDto>> GetBooksWithAuthorsPaged(int page, int pageSize)
    {
        return await _db.Books
            .OrderBy(b => b.BookId)
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
