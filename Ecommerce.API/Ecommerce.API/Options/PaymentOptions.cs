namespace Ecommerce.API.Options
{
    public class PaymentOptions
    {
        public int ConcurrentLimit { get; set; } = 3;
        public int QueueLimit { get; set; } = 2;
    }
}