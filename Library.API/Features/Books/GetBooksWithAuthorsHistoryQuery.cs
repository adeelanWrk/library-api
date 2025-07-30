using Library.API.Data;
using Library.API.DTO.ResultDTO;
using Library.API.DTOs.History;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Library.API.Features.Books
{
    public record GetBooksWithAuthorsHistoryQuery(RequestParamHistoryDto Parameters)
        : IRequest<ResultDTO<List<BookWithAuthorsHistoryDto>>>;

    public class GetBooksWithAuthorsHistoryHandler
        : IRequestHandler<GetBooksWithAuthorsHistoryQuery, ResultDTO<List<BookWithAuthorsHistoryDto>>>
    {
        private readonly LibraryDbContext _db;

        public GetBooksWithAuthorsHistoryHandler(LibraryDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<ResultDTO<List<BookWithAuthorsHistoryDto>>> Handle(
            GetBooksWithAuthorsHistoryQuery request, CancellationToken cancellationToken)
        {
            var currentBooks = await GetCurrentBooksAsync(request.Parameters, cancellationToken);
            var historyBooks = await GetHistoricalBooksAsync(request.Parameters, cancellationToken);

            var combined = currentBooks
                .Concat(historyBooks)
                .OrderByDescending(x => x.UpdatedDate)
                .ToList();

            return ResultSuccess(combined);
        }

        private async Task<List<BookWithAuthorsHistoryDto>> GetCurrentBooksAsync(RequestParamHistoryDto parameters, CancellationToken cancellationToken)
        {
            return await _db.Books
                .Join(_db.BookAuthors, b => b.BookId, ba => ba.BookId, (b, ba) => new { b, ba })
                .Join(_db.Authors, x => x.ba.AuthorId, a => a.AuthorId, (x, a) => new { x.b, a })
                .GroupBy(x => new { x.b.BookId, x.b.UpdatedDate, x.b.Title, x.b.Publisher, x.b.Price })
                .Where(g => g.Key.BookId == parameters.bookId || parameters.bookId == 0)
                .Where(g => g.Any(x => x.a.AuthorId == parameters.authorId) || parameters.authorId == 0)
                .Select(g => new BookWithAuthorsHistoryDto
                {
                    UpdatedDate = g.Key.UpdatedDate,
                    Title = g.Key.Title,
                    Publisher = g.Key.Publisher,
                    Price = g.Key.Price,
                    Authors = string.Join(", ", g.Select(x => x.a.FirstName + " " + x.a.LastName)),
                    AuthorCount = g.Count()
                })
                .ToListAsync(cancellationToken);
        }

        private async Task<List<BookWithAuthorsHistoryDto>> GetHistoricalBooksAsync(RequestParamHistoryDto parameters, CancellationToken cancellationToken)
        {
            return await _db.BooksHistory
                .AsNoTracking()
                .Join(_db.BookAuthors.AsNoTracking(), b => b.BookId, ba => ba.BookId, (b, ba) => new { b, ba })
                .Join(_db.AuthorsHistory.AsNoTracking(), x => x.ba.AuthorId, a => a.AuthorId, (x, a) => new { x.b, a })
                .GroupBy(x => new { x.b.BookId, x.b.UpdatedDate, x.b.Title, x.b.Publisher, x.b.Price })
                .Where(g => g.Key.BookId == parameters.bookId || parameters.bookId == 0)
                .Where(g => g.Any(x => x.a.AuthorId == parameters.authorId) || parameters.authorId == 0)
                .Select(g => new BookWithAuthorsHistoryDto
                {
                    UpdatedDate = g.Key.UpdatedDate,
                    Title = g.Key.Title,
                    Publisher = g.Key.Publisher,
                    Price = g.Key.Price,
                    Authors = string.Join(", ", g.Select(x => x.a.FirstName + " " + x.a.LastName)),
                    AuthorCount = g.Count()
                })
                .OrderByDescending(x => x.UpdatedDate)
                .ToListAsync(cancellationToken);
        }

        private ResultDTO<List<BookWithAuthorsHistoryDto>> ResultSuccess(List<BookWithAuthorsHistoryDto> data)
        {
            return new ResultDTO<List<BookWithAuthorsHistoryDto>>
            {
                Data = data,
                Desc = null,
                IsError = false,
                StatusCode = 200,
                ErrorMessage = null
            };
        }
    }
}
