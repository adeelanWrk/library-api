using Library.API.Features.Books;
using MediatR;

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

    }
}