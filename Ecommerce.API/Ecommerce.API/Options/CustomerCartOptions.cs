namespace Ecommerce.API.Options
{
    public class CustomerCartOptions
    {
        public int TokenLimit { get; set; } = 50;
        public int ReplenishmentPeriodMinutes { get; set; } = 1;
        public int TokensPerPeriod { get; set; } = 15;
        public int QueueLimit { get; set; } = 5;
    }
}