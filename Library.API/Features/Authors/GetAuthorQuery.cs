using Library.API.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Library.API.Features.Authors
{
    public record GetAuthorQuery()
        : IRequest<List<AuthorDto>>;

    public class GetAuthorHandler : IRequestHandler<GetAuthorQuery, List<AuthorDto>>
    {
        private readonly LibraryDbContext _db;

        public GetAuthorHandler(LibraryDbContext db)
        {
            _db = db;
        }

        public async Task<List<AuthorDto>> Handle(GetAuthorQuery request, CancellationToken cancellationToken)
        {
            var authors = await _db.Authors
                .AsNoTracking()
                .OrderBy(a => a.FirstName)
                .Select(a => new AuthorDto(
                    a.AuthorId,
                    a.FirstName,
                    a.LastName,
                    a.PenName
                ))
                .ToListAsync(cancellationToken);

            return authors;
        }
    }
}
