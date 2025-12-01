namespace Ecommerce.API.Options
{
    public class ProductsOptions
    {
        public int PermitLimit { get; set; } = 100;
        public int WindowMinutes { get; set; } = 1;
        public int SegmentsPerWindow { get; set; } = 6;
        public int QueueLimit { get; set; } = 10;
    }
}