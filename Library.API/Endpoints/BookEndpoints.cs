using Library.API.Features.Books;
using Library.API.Features.RawData;
using MediatR;
public record ImportRawDataRequest(string FileName, string FileBase64);

public static class BookEndpoints
{
    public static void MapBookEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/books", async (
            GetBooksPagedRequestDto request,
            ISender sender) =>
                    {
                        var query = new GetBooksWithAuthorsPagedQuery(
                            request
                        );

                        var result = await sender.Send(query);
                        return Results.Ok(result);
                    });

        app.MapGet("/api/books/infinite", async (
            ISender sender) =>
        {
            var query = new GetBooksQuery();
            var result = await sender.Send(query);
            return Results.Ok(result);
        });

        app.MapPost("/api/books/mui", async (
            GetBooksPagedRequestDto request,
            ISender sender) =>
        {
            var query = new GetBooksWithAuthorsMuiQuery(request);
            var result = await sender.Send(query);
            return Results.Ok(result);
        });
        app.MapGet("/api/books/export-raw-data", async (
            ISender sender) =>
        {
            var query = new ExportRawDataQuery();
            var result = await sender.Send(query);
            if (result.Data == null)
                return Results.NotFound("Exported data not found.");
            return Results.File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "rawData.xlsx");
        });

        app.MapPost("/api/books/import-raw-data", async (
            ImportRawDataRequest request,
            ISender sender) =>
        {
            if (string.IsNullOrEmpty(request.FileBase64))
                return Results.BadRequest("FileBase64 is required.");

            byte[] fileBytes;
            try
            {
                fileBytes = Convert.FromBase64String(request.FileBase64);
            }
            catch
            {
                return Results.BadRequest("Invalid Base64 string.");
            }

            var stream = new MemoryStream(fileBytes);
            var formFile = new FormFile(stream, 0, stream.Length, "file", request.FileName);

            var command = new ImportRawDataCmd(formFile);

            var result = await sender.Send(command);
            if (result.StatusCode != 200)
                return Results.BadRequest(result.Desc);

            return Results.Ok(result);
        });

        // app.MapPost("/api/books/import-raw-data", async (
        //     IFormFile file,
        //     ISender sender) =>
        // {
        //     if (file == null || file.Length == 0)
        //         return Results.BadRequest("File is required.");

        //     var command = new ImportRawDataCmd(file);
        //     var result = await sender.Send(command);
        //     if (result.StatusCode != 200)
        //         return Results.BadRequest(result.Desc);

        //     return Results.Ok(result);
        // });
    }
}
