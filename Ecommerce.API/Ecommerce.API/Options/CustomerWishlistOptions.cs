namespace Ecommerce.API.Options
{
    public class CustomerWishlistOptions
    {
        public int TokenLimit { get; set; } = 30;
        public int ReplenishmentPeriodMinutes { get; set; } = 1;
        public int TokensPerPeriod { get; set; } = 10;
        public int QueueLimit { get; set; } = 5;
    }
}