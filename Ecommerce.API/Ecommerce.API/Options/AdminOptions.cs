namespace Ecommerce.API.Options
{
    public class AdminOptions
    {
        public int PermitLimit { get; set; } = 50;
        public int WindowMinutes { get; set; } = 1;
        public int SegmentsPerWindow { get; set; } = 6;
        public int QueueLimit { get; set; } = 5;
    }
}