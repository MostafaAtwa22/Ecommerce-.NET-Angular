namespace Ecommerce.API.Dtos.Responses
{
    public class CouponResponseDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public DateTimeOffset ExpiryDate { get; set; }
        public bool IsActive { get; set; }
    }
}
