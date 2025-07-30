using Evacuation.DTO.ResultSeverSideDTO;
using Library.API.Data;
using Library.API.DTOs.book.BookWithAuthorsMuiDto;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Library.API.Features.Books
{
    public record GetBooksWithAuthorsMuiQuery(GetBooksPagedRequestDto Parameters)
        : IRequest<ResultSeverSideDTO<List<BookWithAuthorsMuiDto>>>;

    public class GetBooksWithAuthorsMuiHandler : IRequestHandler<GetBooksWithAuthorsMuiQuery, ResultSeverSideDTO<List<BookWithAuthorsMuiDto>>>
    {
        private readonly LibraryDbContext _db;

        private static readonly HashSet<string> AllowedSortBy = new()
        {
            "title", "authorCount", "authors"
        };

        private static readonly HashSet<string> AllowedSortDirection = new()
        {
            "ASC", "DESC"
        };

        public GetBooksWithAuthorsMuiHandler(LibraryDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<ResultSeverSideDTO<List<BookWithAuthorsMuiDto>>> Handle(GetBooksWithAuthorsMuiQuery request, CancellationToken cancellationToken)
        {
            ValidateRequest(request.Parameters);

            var booksQuery = BuildBooksQuery(request.Parameters);
            int totalCount = await booksQuery.CountAsync(cancellationToken);

            var pagedBooks = await ApplyPaging(booksQuery, request.Parameters)
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var result = pagedBooks.Select((b, index) => new BookWithAuthorsMuiDto
            {
                
                BookId = b.BookId,
                Title = b.Title ?? string.Empty,
                Publisher = b.Publisher ?? string.Empty,
                Price = b.Price,
                Authors = b.BookAuthors.Select(ba => new AuthorsDto
                {
                    AuthorId = ba.Author.AuthorId,
                    FirstName = ba.Author.FirstName,
                    LastName = ba.Author.LastName
                }).ToList(),
                AuthorCount = b.BookAuthors.Count()
            }).ToList();

            return new ResultSeverSideDTO<List<BookWithAuthorsMuiDto>>
            {
                Data = result,
                Desc = null,
                IsError = false,
                StatusCode = 200,
                ErrorMessage = null,
                TotalCount = totalCount,
                Page = request.Parameters.Page,
                PageSize = request.Parameters.PageSize,
            };
        }

        private void ValidateRequest(GetBooksPagedRequestDto parameters)
        {
            if (parameters.Page <= 0)
                ThrowArgument("Page must be greater than 0");

            if (parameters.PageSize <= 0)
                ThrowArgument("PageSize must be greater than 0");

            var sortBy = (parameters.SortBy ?? "title").Trim();
            if (!AllowedSortBy.Contains(sortBy))
                ThrowArgument($"SortBy must be one of: {string.Join(", ", AllowedSortBy)}");

            var sortDirection = (parameters.SortDirection ?? "ASC").Trim().ToUpperInvariant();
            if (!AllowedSortDirection.Contains(sortDirection))
                ThrowArgument("SortDirection must be 'ASC' or 'DESC'");
        }

        private IQueryable<Book> BuildBooksQuery(GetBooksPagedRequestDto parameters)
        {
            var sortBy = (parameters.SortBy ?? "title").Trim();
            var sortDirection = (parameters.SortDirection ?? "ASC").Trim().ToUpperInvariant();
            var authorId = parameters.AuthorId;

            IQueryable<Book> query = _db.Books;

            if (authorId > 0)
                query = query.Where(b => b.BookAuthors.Any(ba => ba.AuthorId == authorId));

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
                "title" => descending ? query.OrderByDescending(b => b.Title) : query.OrderBy(b => b.Title),
                "authorCount" => descending
                    ? query.OrderByDescending(b => b.BookAuthors.Count)
                    : query.OrderBy(b => b.BookAuthors.Count),
                "authors" => descending
                    ? query.OrderByDescending(b => b.BookAuthors.Select(ba => ba.Author.FirstName).FirstOrDefault())
                    : query.OrderBy(b => b.BookAuthors.Select(ba => ba.Author.FirstName).FirstOrDefault()),
                _ => throw new NotImplementedException()
            };
        }

        private void ThrowArgument(string message) =>
            throw new ArgumentException(message);
    }
}
