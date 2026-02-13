using System.ComponentModel.DataAnnotations;

namespace Ecommerce.API.Dtos.Requests
{
    public class MessageRequestDto
    {
        [MaxLength(5000)]
        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public string ReciverId { get; set; } = string.Empty;

        public string? AttachmentUrl { get; set; }
        public string? AttachmentName { get; set; }
        public string? AttachmentType { get; set; }
    }
}