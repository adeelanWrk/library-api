using ClosedXML.Excel;
using Library.API.DTO.ResultDTO;
using Library.API.DTOs.RawData;
using MediatR;

using System.Text;

namespace Library.API.Features.RawData
{
    public record ImportRawDataCmd(IFormFile File) : IRequest<ResultDTO<string>>;

    public class GetImportRawDataHandler : IRequestHandler<ImportRawDataCmd, ResultDTO<string>>
    {
        private readonly ISender _sender;

        public GetImportRawDataHandler(ISender sender)
        {
            _sender = sender;
        }

        public async Task<ResultDTO<string>> Handle(ImportRawDataCmd request, CancellationToken cancellationToken)
        {
            var rawData = ReadExcelData(request.File, out var validationErrors);

            if (rawData == null || !rawData.Any())
                return Result("No data found in the worksheet.", "", 400);

            if (validationErrors.Length > 0)
                return Result($"Validation failed.<br>{validationErrors}", string.Empty, 400);


            var  result = await _sender.Send(new UpsertRawDataListCmd(rawData), cancellationToken);
            

            return Result($"Import successful.  <br> {result.Desc}", "", result.StatusCode);
        }

        private List<BookAuthorRawDataDto> ReadExcelData(IFormFile file, out StringBuilder validationErrors)
        {
            validationErrors = new StringBuilder();

            using var workbook = new XLWorkbook(file.OpenReadStream());
            var worksheet = workbook.Worksheet(2);
            var range = worksheet.RangeUsed();

            if (range == null)
                return new List<BookAuthorRawDataDto>();

            var rows = range.RowsUsed().Skip(1);
            var result = new List<BookAuthorRawDataDto>();
            int rowIndex = 2;

            foreach (var row in rows)
            {
                var bookIdStr = row.Cell(1).GetValue<string>();
                var priceStr = row.Cell(4).GetValue<string>();
                var authorIdStr = row.Cell(5).GetValue<string>();

                bool hasError = false;

                if (!int.TryParse(bookIdStr, out int bookId))
                {
                    validationErrors.AppendLine($"on column: BookId row: {rowIndex} value: '{bookIdStr}' is not correct <br>");
                    hasError = true;
                }
                if (!decimal.TryParse(priceStr, out decimal price))
                {
                    validationErrors.AppendLine($"on column: Price row: {rowIndex} value: '{priceStr}' is not correct <br>");
                    hasError = true;
                }
                if (!int.TryParse(authorIdStr, out int authorId))
                {
                    validationErrors.AppendLine($"on column: AuthorId row: {rowIndex} value: '{authorIdStr}' is not correct <br>");
                    hasError = true;
                }

                if (!hasError)
                {
                    result.Add(new BookAuthorRawDataDto
                    {
                        BookId = bookId,
                        Title = row.Cell(2).GetValue<string>(),
                        Publisher = row.Cell(3).GetValue<string>(),
                        Price = price,
                        AuthorId = authorId,
                        FirstName = row.Cell(6).GetValue<string>(),
                        LastName = row.Cell(7).GetValue<string>(),
                        PenName = row.Cell(8).GetValue<string>()
                    });
                }

                rowIndex++;
            }

            return result;
        }

        private ResultDTO<string> Result(string desc, string data, int code) =>
            new ResultDTO<string>
            {
                Desc = desc,
                Data = data,
                StatusCode = code
            };
    }
}

