using Library.API.Data;
using Library.API.DTOs.RawData;
using Library.API.DTO.ResultDTO;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Library.API.Features.RawData
{
    public record UpsertRawDataListCmd(List<BookAuthorRawDataDto> RawData) : IRequest<ResultDTO<string>>;

    public class UpsertRawDataListHandler : IRequestHandler<UpsertRawDataListCmd, ResultDTO<string>>
    {
        private readonly LibraryDbContext _db;
        private readonly List<BookHistory> _bookHistories = new();
        private readonly List<AuthorHistory> _authorHistories = new();

        public UpsertRawDataListHandler(LibraryDbContext db)
        {
            _db = db;
        }

        public async Task<ResultDTO<string>> Handle(UpsertRawDataListCmd request, CancellationToken cancellationToken)
        {
            var now = DateTime.Now;
            var validData = GetValidData(request.RawData);

            var bookIds = validData.Select(d => d.BookId).Distinct().ToList();
            var authorIds = validData.Select(d => d.AuthorId).Distinct().ToList();

            var existingBooks = await _db.Books
                .Where(b => bookIds.Contains(b.BookId))
                .ToListAsync(cancellationToken);

            var existingAuthors = await _db.Authors
                .Where(a => authorIds.Contains(a.AuthorId))
                .ToListAsync(cancellationToken);

            var missingBookIds = bookIds.Except(existingBooks.Select(b => b.BookId)).ToList();
            var missingAuthorIds = authorIds.Except(existingAuthors.Select(a => a.AuthorId)).ToList();

            if (missingBookIds.Any() || missingAuthorIds.Any())
                return BuildValidationError(missingBookIds, missingAuthorIds);

            var bookDict = existingBooks.ToDictionary(b => b.BookId);
            var authorDict = existingAuthors.ToDictionary(a => a.AuthorId);

            var counters = UpdateEntitiesWithHistory(validData, bookDict, authorDict, now);

            if (_bookHistories.Any())
                _db.BooksHistory.AddRange(_bookHistories);

            if (_authorHistories.Any())
                _db.AuthorsHistory.AddRange(_authorHistories);

            await _db.SaveChangesAsync(cancellationToken);

            return BuildSuccessResult(counters);
        }

        private static List<BookAuthorRawDataDto> GetValidData(IEnumerable<BookAuthorRawDataDto> data) =>
            data.Where(d => d.BookId != 0 && d.AuthorId != 0).ToList();

        private UpdateCounters UpdateEntitiesWithHistory(
            IEnumerable<BookAuthorRawDataDto> data,
            Dictionary<int, Book> bookDict,
            Dictionary<int, Author> authorDict,
            DateTime now)
        {
            var counters = new UpdateCounters();

            foreach (var dto in data)
            {
                if (bookDict.TryGetValue(dto.BookId, out var book))
                {
                    BackupBookToHistory(book);
                    UpdateBook(book, dto, now);
                    counters.UpdatedBooks++;
                }

                if (authorDict.TryGetValue(dto.AuthorId, out var author))
                {
                    BackupAuthorToHistory(author);
                    UpdateAuthor(author, dto, now);
                    counters.UpdatedAuthors++;
                }
            }

            return counters;
        }

        private void BackupBookToHistory(Book book)
        {
            _bookHistories.Add(new BookHistory
            {
                BookId = book.BookId,
                Title = book.Title,
                Publisher = book.Publisher,
                Price = book.Price,
                UpdatedDate = book.UpdatedDate
            });
        }

        private void BackupAuthorToHistory(Author author)
        {
            _authorHistories.Add(new AuthorHistory
            {
                AuthorId = author.AuthorId,
                FirstName = author.FirstName,
                LastName = author.LastName,
                PenName = author.PenName,
                UpdatedDate = author.UpdatedDate
            });
        }

        private void UpdateBook(Book book, BookAuthorRawDataDto dto, DateTime updatedDate)
        {
            book.Title = dto.Title?.Trim() ?? string.Empty;
            book.Publisher = dto.Publisher?.Trim();
            book.Price = dto.Price;
            book.UpdatedDate = updatedDate;
        }

        private void UpdateAuthor(Author author, BookAuthorRawDataDto dto, DateTime updatedDate)
        {
            author.FirstName = dto.FirstName?.Trim() ?? string.Empty;
            author.LastName = dto.LastName?.Trim() ?? string.Empty;
            author.PenName = dto.PenName?.Trim() ?? string.Empty;
            author.UpdatedDate = updatedDate;
        }

        private ResultDTO<string> BuildValidationError(List<int> missingBookIds, List<int> missingAuthorIds)
        {
            var lines = new List<string>();

            if (missingBookIds.Any())
                lines.Add($"❌ Missing BookIds: {string.Join(", ", missingBookIds)}");

            if (missingAuthorIds.Any())
                lines.Add($"❌ Missing AuthorIds: {string.Join(", ", missingAuthorIds)}");

            var message = "<div style='color: red;'>" + string.Join("<br>", lines) + "</div>";

            return new ResultDTO<string>
            {
                Data = null,
                Desc = message,
                IsError = true,
                StatusCode = 400,
                ErrorMessage = "Validation failed: Some records not found in the database."
            };
        }

        private ResultDTO<string> BuildSuccessResult(UpdateCounters counters)
        {
            var message = $@"
                <div style='color: #007bff;'>
                    ✏️ Updated →
                    <strong>Books:</strong> {counters.UpdatedBooks},
                    <strong>Authors:</strong> {counters.UpdatedAuthors}
                </div>";

            return new ResultDTO<string>
            {
                Data = "Raw data processed successfully.",
                Desc = message,
                IsError = false,
                StatusCode = 200
            };
        }

        private class UpdateCounters
        {
            public int UpdatedBooks { get; set; }
            public int UpdatedAuthors { get; set; }
        }
    }
}
