using Library.API.DTO.ResultDTO;
using Library.API.Data;
using Library.API.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Library.API.Features.Books
{
    public record GetBooksQuery()
        : IRequest<ResultDTO<List<Book>>>;

    public class GetBooksQueryHandler : IRequestHandler<GetBooksQuery, ResultDTO<List<Book>>>
    {
        private readonly LibraryDbContext _db;

        public GetBooksQueryHandler(LibraryDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<ResultDTO<List<Book>>> Handle(GetBooksQuery request, CancellationToken cancellationToken)
        {
            var query = await _db.Books.AsNoTracking().OrderBy(b => b.Publisher)
                .ToListAsync(cancellationToken);
                return new ResultDTO<List<Book>>
                {
                    Data = query,
                    StatusCode = 200,
                    IsError = false,
                    Desc = "Books retrieved successfully"
                };
        }

    }   
}
