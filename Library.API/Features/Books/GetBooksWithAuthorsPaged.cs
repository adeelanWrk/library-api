using Library.API.Data;
using Library.API.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Library.API.Features.Books
{
    public record GetBooksWithAuthorsPagedQuery(int Page, int PageSize, string SortBy, string SortDirection)
        : IRequest<List<BookWithAuthorsDto>>;

    public class GetBooksWithAuthorsPagedHandler : IRequestHandler<GetBooksWithAuthorsPagedQuery, List<BookWithAuthorsDto>>
    {
        private readonly LibraryDbContext _db;
        private static readonly HashSet<string> ValidSortByColumns = new()
        {
            "Title", "Publisher", "Price", "AuthorFirstName", "AuthorLastName"
        };
        private static readonly HashSet<string> ValidSortDirections = new()
        {
            "ASC", "DESC"
        };

        public GetBooksWithAuthorsPagedHandler(LibraryDbContext db) => _db = db;

        public async Task<List<BookWithAuthorsDto>> Handle(GetBooksWithAuthorsPagedQuery request, CancellationToken cancellationToken)
        {
            ValidateRequest(request);

            var books = await FetchBooks(request, cancellationToken);

            return books;
        }

        private void ValidateRequest(GetBooksWithAuthorsPagedQuery request)
        {
            if (request.Page <= 0)
                ThrowArgument("Page must be greater than 0");

            if (request.PageSize <= 0)
                ThrowArgument("PageSize must be greater than 0");

            if (!ValidSortByColumns.Contains(request.SortBy))
                ThrowArgument($"SortBy must be one of: {string.Join(", ", ValidSortByColumns)}");

            if (!ValidSortDirections.Contains(request.SortDirection.ToUpper()))
                ThrowArgument("SortDirection must be 'ASC' or 'DESC'");
        }

        private void ThrowArgument(string message) => throw new ArgumentException(message);

        private async Task<List<BookWithAuthorsDto>> FetchBooks(GetBooksWithAuthorsPagedQuery request, CancellationToken cancellationToken)
        {
            return await _db.Set<BookWithAuthorsDto>()
                .FromSqlRaw(
                    "EXEC dbo.GetBooksWithAuthorsPaged @Page = {0}, @PageSize = {1}, @SortBy = {2}, @SortDirection = {3}",
                    request.Page, request.PageSize, request.SortBy, request.SortDirection.ToUpper())
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}
