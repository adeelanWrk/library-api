public class BookAuthorHistory
{
    public int BookId { get; set; }
    public BookHistory Book { get; set; } = null!;
    public int AuthorId { get; set; }
    public AuthorHistory Author { get; set; } = null!;
    public DateTime? UpdatedDate { get; set; }

}