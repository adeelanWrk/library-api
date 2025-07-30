using Library.API.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Library.API.Features.Authors
{
    public record GetAuthorDropdownQuery(string? Term = null)
        : IRequest<List<AuthorDto>>;

    public record AuthorDto(int AuthorId, string FirstName, string LastName, string? PenName);

    public class GetAuthorDropdownHandler : IRequestHandler<GetAuthorDropdownQuery, List<AuthorDto>>
    {
        private readonly LibraryDbContext _db;

        public GetAuthorDropdownHandler(LibraryDbContext db) => _db = db;

        public async Task<List<AuthorDto>> Handle(GetAuthorDropdownQuery request, CancellationToken cancellationToken)
        {
            var query = _db.Authors
                .AsNoTracking();
                
            if (!string.IsNullOrEmpty(request.Term))
            {
                var term = $"%{request.Term}%";
                query = query.Where(a =>
                    EF.Functions.Like(a.FirstName + " " + a.LastName + (a.PenName != null ? " " + a.PenName : ""), term)
                );
            }


            var authors = await query
                .OrderBy(a => a.FirstName)
                .Take(50)
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
