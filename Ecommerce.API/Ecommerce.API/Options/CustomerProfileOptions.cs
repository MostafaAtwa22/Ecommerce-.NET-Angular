namespace Ecommerce.API.Options
{
    public class CustomerProfileOptions
    {
        public int PermitLimit { get; set; } = 20;
        public int WindowMinutes { get; set; } = 1;
        public int SegmentsPerWindow { get; set; } = 4;
        public int QueueLimit { get; set; } = 3;
    }
}