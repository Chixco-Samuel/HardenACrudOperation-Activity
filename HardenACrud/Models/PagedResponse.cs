namespace HardenACrud.Models
{
    public class PagedResponse<T>
    {
        public IReadOnlyList<T> Data { get; set; } = Array.Empty<T>();
        public PageMeta Meta { get; set; } = new();
    }

    public class PageMeta
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set;  }
        public int PageCount { get; set; }
    }
}
