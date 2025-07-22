namespace Library.API.DTOs.Core;
public class DefualtPagedRequestSeverSideDto
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
}
