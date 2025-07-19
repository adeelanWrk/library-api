using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

public static class BookEndpoints
{
    public static void MapBookEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/books", async (
            int page,
            int pageSize,
            string? sortBy,
            string? sortDirection,
            ISender sender) =>
        {
            var query = new GetBooksWithAuthorsPagedQuery(
                page, pageSize,
                sortBy ?? "bookId",
                sortDirection ?? "asc");
            var result = await sender.Send(query);
            return Results.Ok(result);
        });
    }
}