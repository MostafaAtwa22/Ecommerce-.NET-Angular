namespace Ecommerce.API.Dtos.Responses
{
    public class OnlineUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string ConnectionId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
        public int UnReadCount { get; set; }
    }
}