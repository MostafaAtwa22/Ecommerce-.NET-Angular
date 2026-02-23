namespace Ecommerce.API.Options
{
    public class AuthenticatedOptions
    {
        public int TokenLimit { get; set; } = 100;
        public int ReplenishmentPeriodMinutes { get; set; } = 1;
        public int TokensPerPeriod { get; set; } = 20;
        public int QueueLimit { get; set; } = 5;
    }
}