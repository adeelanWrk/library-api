using ClosedXML.Excel;
using Evacuation.DTO.ResultDTO;
using Library.API.Data;
using Library.API.DTOs.RawData;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Library.API.Features.RawData
{
    public record UpsertRawDataListCmd(List<BookAuthorRawDataDto> RawData) : IRequest<(int Created, int Updated)>;

    public class UpsertRawDataListHandler : IRequestHandler<UpsertRawDataListCmd, (int Created, int Updated)>
    {
        private readonly LibraryDbContext _db;

        public UpsertRawDataListHandler(LibraryDbContext db)
        {
            _db = db;
        }

        public async Task<(int Created, int Updated)> Handle(UpsertRawDataListCmd request, CancellationToken cancellationToken)
        {
            int created = 0, updated = 0;

            foreach (var dto in request.RawData)
            {
                var (bookCreated, bookUpdated) = await UpsertBookAsync(dto, cancellationToken);
                var (authorCreated, authorUpdated) = await UpsertAuthorAsync(dto, cancellationToken);
                var bookAuthorCreated = await AddBookAuthorIfNotExistsAsync(dto, cancellationToken);

                if (bookCreated) created++;
                if (bookUpdated) updated++;
                if (authorCreated) created++;
                if (authorUpdated) updated++;
                if (bookAuthorCreated) created++;
            }

            await _db.SaveChangesAsync(cancellationToken);
            return (created, updated);
        }

        private async Task<(bool Created, bool Updated)> UpsertBookAsync(BookAuthorRawDataDto dto, CancellationToken ct)
        {
            var book = await _db.Books.FindAsync(new object[] { dto.BookId }, ct);

            if (book == null)
            {
                _db.Books.Add(new Book
                {
                    BookId = dto.BookId,
                    Title = dto.Title ?? string.Empty,
                    Publisher = dto.Publisher,
                    Price = dto.Price
                });
                return (true, false);
            }

            book.Title = dto.Title;
            book.Publisher = dto.Publisher;
            book.Price = dto.Price;
            return (false, true);
        }

        private async Task<(bool Created, bool Updated)> UpsertAuthorAsync(BookAuthorRawDataDto dto, CancellationToken ct)
        {
            var author = await _db.Authors.FindAsync(new object[] { dto.AuthorId }, ct);

            if (author == null)
            {
                _db.Authors.Add(new Author
                {
                    AuthorId = dto.AuthorId,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    PenName = dto.PenName
                });
                return (true, false);
            }

            author.FirstName = dto.FirstName;
            author.LastName = dto.LastName;
            author.PenName = dto.PenName;
            return (false, true);
        }

        private async Task<bool> AddBookAuthorIfNotExistsAsync(BookAuthorRawDataDto dto, CancellationToken ct)
        {
            var exists = await _db.BookAuthors.FindAsync(new object[] { dto.BookId, dto.AuthorId }, ct);
            if (exists != null) return false;

            _db.BookAuthors.Add(new BookAuthor
            {
                BookId = dto.BookId,
                AuthorId = dto.AuthorId
            });
            return true;
        }
    }
}

