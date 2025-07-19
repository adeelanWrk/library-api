public record GetBooksWithAuthorsPagedQuery(int Page, int PageSize, string SortBy, string SortDirection)
    : IRequest<List<BookWithAuthorsDto>>;

public class GetBooksWithAuthorsPagedHandler : IRequestHandler<GetBooksWithAuthorsPagedQuery, List<BookWithAuthorsDto>>
{
    private readonly LibraryDbContext _db;
    public GetBooksWithAuthorsPagedHandler(LibraryDbContext db) => _db = db;

    public async Task<List<BookWithAuthorsDto>> Handle(GetBooksWithAuthorsPagedQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Book> query = _db.Books.Include(b => b.BookAuthors).ThenInclude(ba => ba.Author);

        query = request.SortBy.ToLower() switch
        {
            "title" => request.SortDirection.ToLower() == "desc" ? query.OrderByDescending(b => b.Title) : query.OrderBy(b => b.Title),
            "publisher" => request.SortDirection.ToLower() == "desc" ? query.OrderByDescending(b => b.Publisher) : query.OrderBy(b => b.Publisher),
            "price" => request.SortDirection.ToLower() == "desc" ? query.OrderByDescending(b => b.Price) : query.OrderBy(b => b.Price),
            _ => query.OrderBy(b => b.BookId)
        };

        return await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(b => new BookWithAuthorsDto
            {
                Title = b.Title,
                Authors = b.BookAuthors.Select(ba => ba.Author.PenName ?? ba.Author.FirstName + " " + ba.Author.LastName).ToList(),
                AuthorCount = b.BookAuthors.Count
            }).ToListAsync(cancellationToken);
    }
}