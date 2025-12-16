namespace Ecommerce.API.Dtos.Responses
{
    public class UserCommonDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string ProfilePicture { get; set; } = string.Empty;
        public ICollection<string> Roles { get; set; } = new List<string>();
    }
}