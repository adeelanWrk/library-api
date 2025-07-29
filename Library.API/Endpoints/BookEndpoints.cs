using Library.API.Features.Books;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
    }
}
