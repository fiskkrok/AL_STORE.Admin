using Admin.Application.Common.Models;

namespace Admin.WebAPI.Models.Responses;

public record PagedResponse<T>
{
    public List<T> Items { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasNextPage { get; init; }
    public bool HasPreviousPage { get; init; }

    public static PagedResponse<T> FromPagedList<TSource>(
        PagedList<TSource> pagedList,
        Func<TSource, T> mapper)
    {
        return new PagedResponse<T>
        {
            Items = pagedList.Items.Select(mapper).ToList(),
            TotalCount = pagedList.TotalCount,
            Page = pagedList.PageNumber,
            PageSize = pagedList.Items.Count,
            TotalPages = pagedList.TotalPages,
            HasNextPage = pagedList.HasNextPage,
            HasPreviousPage = pagedList.HasPreviousPage
        };
    }
}
