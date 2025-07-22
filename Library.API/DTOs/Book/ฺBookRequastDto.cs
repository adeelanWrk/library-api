using Library.API.DTOs.Core;

public class GetBooksPagedRequestDto : DefualtPagedRequestSeverSideDto
{
    public int AuthorId { get; set; }
}
