using Library.API.Data;
using Library.API.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace Library.API.Features.Books
{
    public record GetBooksWithAuthorsPagedQuery(GetBooksPagedRequestDto Parameters)
        : IRequest<List<BookWithAuthorsDto>>;

    public class GetBooksWithAuthorsPagedHandler : IRequestHandler<GetBooksWithAuthorsPagedQuery, List<BookWithAuthorsDto>>
    {
        private readonly LibraryDbContext _db;

        private static readonly HashSet<string> AllowedSortBy = new()
        {
            "Title","AuthorCount","Author", "Publisher", "Price", "AuthorFirstName", "AuthorLastName"
        };

        private static readonly HashSet<string> AllowedSortDirection = new()
        {
            "ASC", "DESC"
        };

        public GetBooksWithAuthorsPagedHandler(LibraryDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<List<BookWithAuthorsDto>> Handle(GetBooksWithAuthorsPagedQuery request, CancellationToken cancellationToken)
        {
            ValidateRequest(request.Parameters);

            var booksQuery = BuildBooksQuery(request.Parameters);

            var pagedBooks = await ApplyPaging(booksQuery, request.Parameters)
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
            var result = pagedBooks.Select(b => new BookWithAuthorsDto
            {
                Title = b.Title,
                Authors = b.BookAuthors.Select(ba => new AuthorsDto
                {
                    AuthorId = ba.Author.AuthorId,
                    FirstName = ba.Author.FirstName,
                    LastName = ba.Author.LastName
                }).ToList(),
                AuthorCount = b.BookAuthors.Count()
            }).ToList();

            return result;
        }

        private void ValidateRequest(GetBooksPagedRequestDto parameters)
        {
            if (parameters.Page <= 0)
                ThrowArgument("Page must be greater than 0");

            if (parameters.PageSize <= 0)
                ThrowArgument("PageSize must be greater than 0");

            var sortBy = (parameters.SortBy ?? "Title").Trim();
            if (!AllowedSortBy.Contains(sortBy))
                ThrowArgument($"SortBy must be one of: {string.Join(", ", AllowedSortBy)}");

            var sortDirection = (parameters.SortDirection ?? "ASC").Trim().ToUpperInvariant();
            if (!AllowedSortDirection.Contains(sortDirection))
                ThrowArgument("SortDirection must be 'ASC' or 'DESC'");
        }

        private IQueryable<Book> BuildBooksQuery(GetBooksPagedRequestDto parameters)
        {
            var sortBy = (parameters.SortBy ?? "Title").Trim();
            var sortDirection = (parameters.SortDirection ?? "ASC").Trim().ToUpperInvariant();
            var authorId = parameters.AuthorId;

            IQueryable<Book> query = _db.Books;

            if (authorId > 0)
            {
                query = query.Where(b => b.BookAuthors.Any(ba => ba.AuthorId == authorId));
            }

            query = ApplySorting(query, sortBy, sortDirection);

            return query;
        }

        private IQueryable<Book> ApplyPaging(IQueryable<Book> query, GetBooksPagedRequestDto parameters)
        {
            var skip = (parameters.Page - 1) * parameters.PageSize;
            return query.Skip(skip).Take(parameters.PageSize);
        }

        private IQueryable<Book> ApplySorting(IQueryable<Book> query, string sortBy, string sortDirection)
        {
            bool descending = sortDirection == "DESC";

            return sortBy switch
            {
                "Title" => descending ? query.OrderByDescending(b => b.Title) : query.OrderBy(b => b.Title),
                "AuthorCount" => descending
                    ? query.OrderByDescending(b => b.BookAuthors.Count)
                    : query.OrderBy(b => b.BookAuthors.Count),
                "Author" => descending
                    ? query.OrderByDescending(b => b.BookAuthors.Select(ba => ba.Author.FirstName).FirstOrDefault())
                    : query.OrderBy(b => b.BookAuthors.Select(ba => ba.Author.FirstName).FirstOrDefault()),
                "Publisher" => descending ? query.OrderByDescending(b => b.Publisher) : query.OrderBy(b => b.Publisher),
                "Price" => descending ? query.OrderByDescending(b => b.Price) : query.OrderBy(b => b.Price),
                "AuthorFirstName" => descending
                    ? query.OrderByDescending(b => b.BookAuthors.Select(ba => ba.Author.FirstName).FirstOrDefault())
                    : query.OrderBy(b => b.BookAuthors.Select(ba => ba.Author.FirstName).FirstOrDefault()),
                "AuthorLastName" => descending
                    ? query.OrderByDescending(b => b.BookAuthors.Select(ba => ba.Author.LastName).FirstOrDefault())
                    : query.OrderBy(b => b.BookAuthors.Select(ba => ba.Author.LastName).FirstOrDefault()),
                _ => query.OrderBy(b => b.Title),
            };
        }

        private void ThrowArgument(string message) =>
            throw new ArgumentException(message);
    }
}
