namespace Ecommerce.API.Options
{
    public class AuthenticationOptions
    {
        public int PermitLimit { get; set; } = 5;
        public int WindowMinutes { get; set; } = 1;
        public int QueueLimit { get; set; } = 2;
    }
}