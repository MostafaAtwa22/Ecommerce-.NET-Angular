
namespace Ecommerce.API.Dtos.Requests
{
    public class ProfileImageUpdateDto
    {
        [AllowedExtensions(FileSettings.AllowedExtensions),
            MaxFileSize(FileSettings.MaxFileSizeInBytes)]
        public IFormFile? ProfileImageFile { get; set; }
    }
}
