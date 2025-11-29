using Ecommerce.API.Helpers.Attributes;
using Ecommerce.Core.Enums;
using Ecommerce.Infrastructure.Settings;

namespace Ecommerce.API.Dtos.Requests
{
    public class ProfileUpdateDto
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; } 

        public string? UserName { get; set; }

        public Gender? Gender { get; set; }

        public string? ProfileImage { get; set; }

        [AllowedExtensions(FileSettings.AllowedExtensions),
            MaxFileSize(FileSettings.MaxFileSizeInBytes)]
        public IFormFile? ProfileImageFile { get; set; }

        public string? PhoneNumber { get; set; } 
    }
}