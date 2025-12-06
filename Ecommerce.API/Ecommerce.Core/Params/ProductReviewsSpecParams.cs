namespace Ecommerce.Core.Params
{
    public class ProductReviewsSpecParams
    {
        private const int MaxPageSize = 20;
        public int PageIndex { get; set; } = 1;

        private int _pageSize = 5;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }
        public string? Sort { get; set; }
    }
}