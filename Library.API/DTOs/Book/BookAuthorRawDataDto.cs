namespace Library.API.DTOs.RawData;

public class BookAuthorRawDataDto
{
    public int BookId { get; set; }
    public string? Title { get; set; }
    public string? Publisher { get; set; }
    public decimal? Price { get; set; }
    public int AuthorId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PenName { get; set; }
}