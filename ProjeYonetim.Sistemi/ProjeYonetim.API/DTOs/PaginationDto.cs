namespace ProjeYonetim.API.DTOs
{
    public class PaginationDto
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 10; // Varsayılan değer

        public int PageNumber { get; set; } = 1; // Varsayılan değer

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }
    }
}