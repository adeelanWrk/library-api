using ClosedXML.Excel;
using Library.API.Data;
using Library.API.DTO.ResultDTO;
using Library.API.DTOs.RawData;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Library.API.Features.RawData
{
    public record ExportRawDataQuery() : IRequest<ResultDTO<byte[]>>;

    public class GetExportRawDataHandler : IRequestHandler<ExportRawDataQuery, ResultDTO<byte[]>>
    {
        private readonly LibraryDbContext _db;
        private readonly IWebHostEnvironment _env;

        public GetExportRawDataHandler(LibraryDbContext db, IWebHostEnvironment env)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        public async Task<ResultDTO<byte[]>> Handle(ExportRawDataQuery request, CancellationToken cancellationToken)
        {
            var data = await GetBookAuthorsAsync(cancellationToken);

            string templatePath = Path.Combine(_env.ContentRootPath, "Asset", "ExcelTemplate", "rawDataTemplate.xlsx");
            
            using var workbook = new XLWorkbook(templatePath);
            var worksheet = workbook.Worksheet(2);

            if (worksheet.AutoFilter != null)
                worksheet.AutoFilter.Clear();

            worksheet.Cell(2, 1).InsertData(data); 

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);

            return new ResultDTO<byte[]>
            {
                Data = stream.ToArray(),
                Desc = "Raw data exported successfully.",
                StatusCode = 200
            };
        }

        private async Task<List<BookAuthorRawDataDto>> GetBookAuthorsAsync(CancellationToken cancellationToken)
        {
            return await _db.BookAuthors
                .Include(ba => ba.Book)
                .Include(ba => ba.Author)
                .OrderBy(ba => ba.BookId).ThenBy(ba => ba.AuthorId)
                .Select(ba => new BookAuthorRawDataDto
                {
                    BookId = ba.Book.BookId,
                    Title = ba.Book.Title,
                    Publisher = ba.Book.Publisher,
                    Price = ba.Book.Price,
                    AuthorId = ba.Author.AuthorId,
                    FirstName = ba.Author.FirstName,
                    LastName = ba.Author.LastName,
                    PenName = ba.Author.PenName
                })
                .AsNoTracking()
                .Take(1000)
                .ToListAsync(cancellationToken);
        }
    }
}
