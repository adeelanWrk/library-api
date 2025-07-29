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

        public UpsertRawDataListHandler(LibraryDbContext db) => _db = db;

        public async Task<(string CreatedSummary, string UpdatedSummary)> Handle(UpsertRawDataListCmd request, CancellationToken cancellationToken)
        {
            var validData = FilterInvalidData(request.RawData);

            var bookIds = ExtractBookIds(validData);
            var authorIds = ExtractAuthorIds(validData);

            var existingBooks = await FetchExistingBooksAsync(bookIds, cancellationToken);
            var existingAuthors = await FetchExistingAuthorsAsync(authorIds, cancellationToken);

            var counters = new UpsertCounters();

            // 1. Upsert Books and Authors first
            foreach (var dto in validData)
            {
                UpsertBook(dto, existingBooks, counters);
                UpsertAuthor(dto, existingAuthors, counters);
            }

            // Save Books and Authors so that IDs are generated and exist in DB
            await _db.SaveChangesAsync(cancellationToken);

            // Reload Books and Authors (if IDs auto-generated, to be sure)
            existingBooks = await FetchExistingBooksAsync(bookIds, cancellationToken);
            existingAuthors = await FetchExistingAuthorsAsync(authorIds, cancellationToken);

            // 2. Upsert BookAuthors now
            var existingBookAuthors = await FetchExistingBookAuthorsAsync(bookIds, authorIds, cancellationToken);

            foreach (var dto in validData)
            {
                UpsertBookAuthorLink(dto, existingBookAuthors, counters);
            }

            await _db.SaveChangesAsync(cancellationToken);

            return BuildSummary(counters);
        }

        private static List<BookAuthorRawDataDto> FilterInvalidData(List<BookAuthorRawDataDto> rawData)
            => rawData.Where(d => d.BookId != 0 && d.AuthorId != 0).ToList();

        private static List<int> ExtractBookIds(IEnumerable<BookAuthorRawDataDto> data)
            => data.Select(d => d.BookId).Distinct().ToList();

        private static List<int> ExtractAuthorIds(IEnumerable<BookAuthorRawDataDto> data)
            => data.Select(d => d.AuthorId).Distinct().ToList();

        private async Task<Dictionary<int, Book>> FetchExistingBooksAsync(List<int> bookIds, CancellationToken ct)
            => await _db.Books.Where(b => bookIds.Contains(b.BookId))
                              .ToDictionaryAsync(b => b.BookId, ct);

        private async Task<Dictionary<int, Author>> FetchExistingAuthorsAsync(List<int> authorIds, CancellationToken ct)
            => await _db.Authors.Where(a => authorIds.Contains(a.AuthorId))
                               .ToDictionaryAsync(a => a.AuthorId, ct);

        private async Task<List<BookAuthor>> FetchExistingBookAuthorsAsync(List<int> bookIds, List<int> authorIds, CancellationToken ct)
            => await _db.BookAuthors.Where(ba => bookIds.Contains(ba.BookId) && authorIds.Contains(ba.AuthorId))
                                   .ToListAsync(ct);

        private void UpsertBook(BookAuthorRawDataDto dto, Dictionary<int, Book> existingBooks, UpsertCounters counters)
        {
            if (!existingBooks.TryGetValue(dto.BookId, out var book))
            {
                book = CreateBookFromDto(dto);
                _db.Books.Add(book);
                existingBooks.Add(dto.BookId, book);
                counters.CreatedBooks++;
                return;
            }

            if (UpdateBookIfChanged(book, dto))
                counters.UpdatedBooks++;
        }

        private static Book CreateBookFromDto(BookAuthorRawDataDto dto)
            => new Book
            {
                // สมมติ BookId เป็น Identity ให้ไม่กำหนด
                // BookId = dto.BookId,
                Title = dto.Title?.Trim() ?? string.Empty,
                Publisher = dto.Publisher?.Trim(),
                Price = dto.Price
            };

        private static bool UpdateBookIfChanged(Book book, BookAuthorRawDataDto dto)
        {
            var title = dto.Title?.Trim() ?? string.Empty;
            var publisher = dto.Publisher?.Trim();

            if (book.Title == title && book.Publisher == publisher && book.Price == dto.Price)
                return false;

            book.Title = title;
            book.Publisher = publisher;
            book.Price = dto.Price;
            return true;
        }

        private void UpsertAuthor(BookAuthorRawDataDto dto, Dictionary<int, Author> existingAuthors, UpsertCounters counters)
        {
            if (!existingAuthors.TryGetValue(dto.AuthorId, out var author))
            {
                author = CreateAuthorFromDto(dto);
                _db.Authors.Add(author);
                existingAuthors.Add(dto.AuthorId, author);
                counters.CreatedAuthors++;
                return;
            }

            if (UpdateAuthorIfChanged(author, dto))
                counters.UpdatedAuthors++;
        }

        private static Author CreateAuthorFromDto(BookAuthorRawDataDto dto)
            => new Author
            {
                // สมมติ AuthorId เป็น Identity ให้ไม่กำหนด
                // AuthorId = dto.AuthorId,
                FirstName = dto.FirstName?.Trim() ?? string.Empty,
                LastName = dto.LastName?.Trim() ?? string.Empty,
                PenName = dto.PenName?.Trim() ?? string.Empty
            };

        private static bool UpdateAuthorIfChanged(Author author, BookAuthorRawDataDto dto)
        {
            var firstName = dto.FirstName?.Trim() ?? string.Empty;
            var lastName = dto.LastName?.Trim() ?? string.Empty;
            var penName = dto.PenName?.Trim() ?? string.Empty;

            if (author.FirstName == firstName && author.LastName == lastName && author.PenName == penName)
                return false;

            author.FirstName = firstName;
            author.LastName = lastName;
            author.PenName = penName;
            return true;
        }

        private void UpsertBookAuthorLink(BookAuthorRawDataDto dto, List<BookAuthor> existingBookAuthors, UpsertCounters counters)
        {
            var exists = existingBookAuthors.Any(ba => ba.BookId == dto.BookId && ba.AuthorId == dto.AuthorId);
            if (exists) return;

            _db.BookAuthors.Add(new BookAuthor { BookId = dto.BookId, AuthorId = dto.AuthorId });
            existingBookAuthors.Add(new BookAuthor { BookId = dto.BookId, AuthorId = dto.AuthorId });
            counters.CreatedBookAuthors++;
        }

        private static (string CreatedSummary, string UpdatedSummary) BuildSummary(UpsertCounters counters)
        {
            var createdSummary = $@"
            <div style='color: green;'>
                <strong>✅ Created</strong> →
                <span><strong>Books:</strong> {counters.CreatedBooks}, </span>
                <span><strong>Authors:</strong> {counters.CreatedAuthors}, </span>
                <span><strong>Links:</strong> {counters.CreatedBookAuthors}</span>
            </div>";

            var updatedSummary = $@"
            <div style='color: #007bff;'>
                <strong>✏️ Updated</strong> →
                <span><strong>Books:</strong> {counters.UpdatedBooks}, </span>
                <span><strong>Authors:</strong> {counters.UpdatedAuthors}</span>
            </div>";

            return (createdSummary, updatedSummary);
        }

        private class UpsertCounters
        {
            public int CreatedBooks { get; set; }
            public int UpdatedBooks { get; set; }
            public int CreatedAuthors { get; set; }
            public int UpdatedAuthors { get; set; }
            public int CreatedBookAuthors { get; set; }
        }
    }
}
