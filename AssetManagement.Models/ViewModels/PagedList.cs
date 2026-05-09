namespace AssetManagement.Models.ViewModels;

public class PagedList<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)Total / PageSize) : 0;
    public bool HasPrev => Page > 1;
    public bool HasNext => Page < TotalPages;
}
