namespace Ecommerce.API.Options
{
    public class CustomerRegisterOptions
    {
        public int PermitLimit { get; set; } = 3;
        public int WindowMinutes { get; set; } = 5;
        public int QueueLimit { get; set; } = 1;
    }
}