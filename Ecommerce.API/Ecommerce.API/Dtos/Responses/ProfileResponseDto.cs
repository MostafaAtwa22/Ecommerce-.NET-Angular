namespace Ecommerce.API.Dtos.Responses
{
    public class ProfileResponseDto : UserCommonDto
    {
        public string Id { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
    }
}