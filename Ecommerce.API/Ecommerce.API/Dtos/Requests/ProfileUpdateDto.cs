using Ecommerce.Core.Enums;

namespace Ecommerce.API.Dtos.Requests
{
    public class ProfileUpdateDto
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; } 

        public string? UserName { get; set; }

        public Gender? Gender { get; set; }
        
        public string? PhoneNumber { get; set; } 
    }
}