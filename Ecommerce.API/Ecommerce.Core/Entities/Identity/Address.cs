
namespace Ecommerce.Core.Entities.Identity
{
    public class Address 
    {
        public int Id { get; set; }

        [Required, MinLength(3), MaxLength(50)]
        public string Country { get; set; } = string.Empty;

        [Required, MinLength(3), MaxLength(60)]
        public string Government { get; set; } = string.Empty;

        [Required, MinLength(3), MaxLength(20)]
        public string City { get; set; } = string.Empty;

        [Required, MinLength(3), MaxLength(100)]
        public string Street { get; set; } = string.Empty;

        [Required, MinLength(3), MaxLength(20)]
        public string Zipcode { get; set; } = string.Empty;

        public string ApplicationUserId { get; set; } = string.Empty;
        public ApplicationUser ApplicationUser { get; set; } = default!;
    }
}
