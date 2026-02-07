using System.ComponentModel.DataAnnotations;

namespace Ecommerce.API.Dtos.Requests
{
    public class MessageRequestDto
    {
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [MaxLength(5000)]
        public string Content { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        [Required]
        public string SenderId { get; set; } = string.Empty;

        [Required]
        public string ReciverId { get; set; } = string.Empty;
    }
}