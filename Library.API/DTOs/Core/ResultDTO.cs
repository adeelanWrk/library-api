namespace Library.API.DTO.ResultDTO;

public class ResultDTO<T>
{
    public T? Data { get; set; } = default;
    public string? Desc { get; set; } = null;
    public bool IsError { get; set; } = false;
    public int StatusCode { get; set; } = 200;
    public string? ErrorMessage { get; set; } = null;
    
}