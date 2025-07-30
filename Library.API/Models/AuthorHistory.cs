using System.Collections.Generic;
public class AuthorHistory
{
    public int Id { get; set; }
    public int AuthorId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string PenName { get; set; } = null!;
    public DateTime? UpdatedDate { get; set; }

    // public ICollection<BookAuthorHistory> BookAuthors { get; set; } = new List<BookAuthorHistory>();
}