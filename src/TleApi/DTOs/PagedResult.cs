namespace TleApi.DTOs;

/// <summary>
/// Paginated result wrapper
/// </summary>
public class PagedResult<T>
{
    public required List<T> Items { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
}
