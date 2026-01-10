namespace EligibilityPlatform.Domain.Models
{
    public class PagingParametersModel
    {
        const int maxPageSize = 50;
        public int PageNumber { get; set; } = 1;
        private int _pageSize = 4;
        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = (value > maxPageSize) ? maxPageSize : value;
            }
        }

        public string? SearchTerm { get; set; } = string.Empty;
        public string OrderBy { get; set; } = string.Empty;
    }
}
