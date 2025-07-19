using System.Collections.Generic;
using System.Threading.Tasks;

public interface IBookRepository
{
    Task<List<BookDto>> GetBooksByAuthorId(int authorId);
    Task<PaginatedResult<BookWithAuthorsDto>> GetAllBooksWithAuthors(int page, int pageSize);
}
