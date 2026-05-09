namespace AssetManagement.Models.ViewModels;

public class PaginationModel
{
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public bool HasPrev { get; set; }
    public bool HasNext { get; set; }
    /// <summary>
    /// Base URL without a page query param. May already contain other query params.
    /// The partial will append ?page=N or &amp;page=N automatically.
    /// </summary>
    public string BaseUrl { get; set; } = "";
}
