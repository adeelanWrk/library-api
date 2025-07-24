using System.Collections.Generic;
namespace Library.API.DTOs
{
    public class BookWithAuthorsDto
    {
        public int Row { get; set; }
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<AuthorsDto> Authors { get; set; } = new();
        public int AuthorCount { get; set; }
    }
    public class AuthorsDto
    {
        public int AuthorId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}