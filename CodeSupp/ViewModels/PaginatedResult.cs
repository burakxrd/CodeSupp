namespace CodeSupp.ViewModels
{
    public class PaginatedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }   // Toplam kayıt sayısı (Filtrelenmiş)
        public int PageNumber { get; set; }   // Şu anki sayfa
        public int PageSize { get; set; }     // Sayfa başına kayıt
        public int TotalPages { get; set; }   // Toplam sayfa sayısı

        public PaginatedResult(List<T> items, int count, int pageNumber, int pageSize)
        {
            Items = items;
            TotalCount = count;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        }
    }
}