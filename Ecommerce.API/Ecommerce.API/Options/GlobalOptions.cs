namespace Ecommerce.API.Options
{
    public class GlobalOptions
    {
        public int TokenLimit { get; set; } = 500;
        public int ReplenishmentPeriodMinutes { get; set; } = 1;
        public int TokensPerPeriod { get; set; } = 100;
        public int QueueLimit { get; set; } = 10;
    }
}