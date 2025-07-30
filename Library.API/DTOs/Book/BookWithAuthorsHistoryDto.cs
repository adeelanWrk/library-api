using System;
namespace Library.API.DTOs.History;
public class BookWithAuthorsHistoryDto
{
    public DateTime? UpdatedDate { get; set; }

    public string? Title { get; set; }

    public string? Publisher { get; set; }

    public decimal? Price { get; set; }

    public string? Authors { get; set; }

    public int? AuthorCount { get; set; }
}