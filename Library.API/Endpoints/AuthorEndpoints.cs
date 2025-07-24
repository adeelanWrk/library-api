using Library.API.Features.Authors;
using MediatR;

public static class AuthorEndpoints
{
    public static void MapAuthorEndpoints(this IEndpointRouteBuilder app)
    {
        // app.MapGet("/api/authors", async (
        //     ISender sender) =>
        // {
        //     var query = new GetAuthorDropdownQuery();
        //     var result = await sender.Send(query);
        //     return Results.Ok(result);
        // });

        app.MapGet("/api/authors/search", async (
            string term,
            ISender sender) =>
        {
            var query = new GetAuthorDropdownQuery(term);
            var result = await sender.Send(query);
            return Results.Ok(result);
        });
    }
}
            