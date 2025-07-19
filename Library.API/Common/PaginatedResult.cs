using System;
using System.Collections.Generic;

public class PaginatedResult<T>
{
    public List<T> Data { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    public PaginatedResult() { }

    public PaginatedResult(List<T> data, int count, int page, int pageSize)
    {
        Data = data;
        TotalCount = count;
        Page = page;
        PageSize = pageSize;
    }

    public static PaginatedResult<T> Create(List<T> items, int count, int page, int pageSize)
        => new(items, count, page, pageSize);
}
