using System.Collections.Generic;

public class Book
{
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
}