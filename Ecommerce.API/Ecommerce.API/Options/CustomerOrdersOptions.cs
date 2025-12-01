namespace Ecommerce.API.Options
{
    public class CustomerOrdersOptions
    {
        public int TokenLimit { get; set; } = 10;
        public int ReplenishmentPeriodMinutes { get; set; } = 1;
        public int TokensPerPeriod { get; set; } = 2;
        public int QueueLimit { get; set; } = 3;
    }
}