public class BooksQueryParams
{
    public int StartRow { get; set; }
    public int EndRow { get; set; }
    public List<SortModelItem>? SortModel { get; set; }
    public Dictionary<string, FilterCondition>? FilterModel { get; set; }
    public string? QuickFilter { get; set; }
}

public class SortModelItem
{
    public string ColId { get; set; } = "";
    public string Sort { get; set; } = ""; // "asc" or "desc"
}

public class FilterCondition
{
    public string FilterType { get; set; } = ""; // "text" | "number" | etc.
    public string? Type { get; set; } // "contains", "equals", etc.
    public string? Filter { get; set; }
    public string? FilterTo { get; set; }
}
