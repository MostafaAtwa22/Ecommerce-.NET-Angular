namespace Ecommerce.API.Dtos.Requests
{
    public class Verify2FADto
    {
        public string Email { get; set; } = null!;
        public string Code { get; set; } = null!;
        public bool RememberMe { get; set; } = false;
    }
}