namespace Ecommerce.API.Dtos.Responses
{
    public class MessageResponseDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string ReciverId { get; set; } = string.Empty;
    }
}