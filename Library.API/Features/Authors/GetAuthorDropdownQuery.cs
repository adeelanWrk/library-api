using Library.API.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Library.API.Features.Authors
{
    public record GetAuthorDropdownQuery()
        : IRequest<List<Author>>;

    public class GetAuthorDropdownHandler : IRequestHandler<GetAuthorDropdownQuery, List<Author>>
    {
        private readonly LibraryDbContext _db;

        public GetAuthorDropdownHandler(LibraryDbContext db) => _db = db;

        public async Task<List<Author>> Handle(GetAuthorDropdownQuery request, CancellationToken cancellationToken)
        {

            var authors = await _db.Authors.OrderBy(a => a.FirstName).AsNoTracking().ToListAsync(cancellationToken);

            return authors;
        }
    }
}
