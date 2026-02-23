namespace Ecommerce.API.Dtos.Responses
{
    public class MessageResponseDto
    {
        public int Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public bool IsReceived { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string ReciverId { get; set; } = string.Empty;
        public bool IsEdited { get; set; }
        public bool IsDeleted { get; set; }

        public string? AttachmentUrl { get; set; }
        public string? AttachmentName { get; set; }
        public string? AttachmentType { get; set; }
    }
}