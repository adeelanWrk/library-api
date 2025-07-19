using System.Collections.Generic;
namespace Library.API.DTOs
{
    public class BookWithAuthorsDto
    {
        public string Title { get; set; } = string.Empty;
        public List<string> Authors { get; set; } = new();
        public int AuthorCount { get; set; }
    }
}