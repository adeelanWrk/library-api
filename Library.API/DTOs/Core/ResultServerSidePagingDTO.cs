namespace Evacuation.DTO.ResultSeverSideDTO;

public class ResultSeverSideDTO<T>
{

    public T? Data { get; set; } = default;
    public string? Desc { get; set; } = null;
    public bool IsError { get; set; } = false;
    public int StatusCode { get; set; } = 200;
    public string? ErrorMessage { get; set; } = null;
    public int TotalCount { get; set; } = 0;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalPages => TotalCount > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    
}