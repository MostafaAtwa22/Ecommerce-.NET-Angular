namespace Ecommerce.API.Dtos.Responses
{
    public class ProductReviewDto
    {
        public int Id { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;
        
        public string LastName { get; set; } = string.Empty;

        public string ProfilePictureUrl { get; set; } = string.Empty;
        
        public string Headline { get; set; } = string.Empty;

        public int HelpfulCount { get; set; }

        public int NotHelpfulCount { get; set; }

        public decimal Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}