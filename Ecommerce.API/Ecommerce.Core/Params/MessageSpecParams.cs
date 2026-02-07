namespace Ecommerce.Core.Params
{
    public class MessageSpecParams
    {
        private const int MaxPageSize = 50;
        public int PageIndex { get; set; } = 1;

        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }

        public string? ReceiverId { get; set; }
        public string? SenderId { get; set; }
    }
}
