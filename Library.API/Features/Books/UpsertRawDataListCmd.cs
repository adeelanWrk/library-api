using ClosedXML.Excel;
using Evacuation.DTO.ResultDTO;
using Library.API.Data;
using Library.API.DTOs.RawData;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Library.API.Features.RawData
{
    public record UpsertRawDataListCmd(List<BookAuthorRawDataDto> RawData) : IRequest<(string CreatedSummary, string UpdatedSummary)>;

    public class UpsertRawDataListHandler : IRequestHandler<UpsertRawDataListCmd, (string CreatedSummary, string UpdatedSummary)>
    {
        private readonly LibraryDbContext _db;

        public UpsertRawDataListHandler(LibraryDbContext db)
        {
            _db = db;
        }

        public async Task<(string CreatedSummary, string UpdatedSummary)> Handle(UpsertRawDataListCmd request, CancellationToken cancellationToken)
        {
            int createdBooks = 0, updatedBooks = 0;
            int createdAuthors = 0, updatedAuthors = 0;
            int createdBookAuthors = 0;

            foreach (var dto in request.RawData)
            {
                if (dto.BookId == 0 || dto.AuthorId == 0)
                    continue;

                var (bookCreated, bookUpdated) = await UpsertBookAsync(dto, cancellationToken);
                // if (bookUpdated) await SaveBookHistoryAsync(dto, cancellationToken);
                if (bookCreated) createdBooks++;
                if (bookUpdated) updatedBooks++;

                var (authorCreated, authorUpdated) = await UpsertAuthorAsync(dto, cancellationToken);
                // if (authorUpdated) await SaveAuthorHistoryAsync(dto, cancellationToken);
                if (authorCreated) createdAuthors++;
                if (authorUpdated) updatedAuthors++;

                var bookAuthorCreated = await AddBookAuthorIfNotExistsAsync(dto, cancellationToken);
                if (bookAuthorCreated)
                {
                    // await SaveBookAuthorHistoryAsync(dto, cancellationToken);
                    createdBookAuthors++;
                }
            }

            await _db.SaveChangesAsync(cancellationToken);
            var createdSummary = $@"
                <div style='color: green;'>
                <strong>✅ Created</strong> →
                <span><strong>Books:</strong> {createdBooks}, </span>
                <span><strong>Authors:</strong> {createdAuthors}, </span>
                <span><strong>Links:</strong> {createdBookAuthors}</span>
                </div>";

            var updatedSummary = $@"
                <div style='color: #007bff;'>
                <strong>✏️ Updated</strong> →
                <span><strong>Books:</strong> {updatedBooks}, </span>
                <span><strong>Authors:</strong> {updatedAuthors}</span>
                </div>";

            return (createdSummary, updatedSummary);
        }


        private async Task<(bool Created, bool Updated)> UpsertBookAsync(BookAuthorRawDataDto dto, CancellationToken ct)
        {
            var book = await _db.Books.FindAsync(new object[] { dto.BookId }, ct);

            if (book == null)
            {
                _db.Books.Add(new Book
                {
                    BookId = dto.BookId,
                    Title = dto.Title?.Trim() ?? string.Empty,
                    Publisher = dto.Publisher?.Trim(),
                    Price = dto.Price
                });
                return (true, false);
            }

            book.Title = dto.Title?.Trim() ?? string.Empty;
            book.Publisher = dto.Publisher?.Trim();
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
                    FirstName = dto.FirstName?.Trim() ?? string.Empty,
                    LastName = dto.LastName?.Trim() ?? string.Empty,
                    PenName = dto.PenName?.Trim() ?? string.Empty
                });
                return (true, false);
            }

            author.FirstName = dto.FirstName?.Trim() ?? string.Empty;
            author.LastName = dto.LastName?.Trim() ?? string.Empty;
            author.PenName = dto.PenName?.Trim() ?? string.Empty;
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

        // private async Task SaveBookHistoryAsync(BookAuthorRawDataDto dto, CancellationToken ct)
        // {
        //     _db.BooksHistory.Add(new BookHistory
        //     {
        //         BookId = dto.BookId,
        //         Title = dto.Title?.Trim() ?? string.Empty,
        //         Publisher = dto.Publisher?.Trim(),
        //         Price = dto.Price,
        //         UpdatedDate = DateTime.Now
        //     });
        // }

        // private async Task SaveAuthorHistoryAsync(BookAuthorRawDataDto dto, CancellationToken ct)
        // {
        //     _db.AuthorsHistory.Add(new AuthorHistory
        //     {
        //         AuthorId = dto.AuthorId,
        //         FirstName = dto.FirstName?.Trim() ?? string.Empty,
        //         LastName = dto.LastName?.Trim() ?? string.Empty,
        //         PenName = dto.PenName?.Trim() ?? string.Empty,
        //         UpdatedDate = DateTime.Now
        //     });
        // }

        // private async Task SaveBookAuthorHistoryAsync(BookAuthorRawDataDto dto, CancellationToken ct)
        // {
        //     _db.BookAuthorsHistory.Add(new BookAuthorHistory
        //     {
        //         BookId = dto.BookId,
        //         AuthorId = dto.AuthorId,
        //         UpdatedDate = DateTime.Now
        //     });
        // }
    }
}
