using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Core.Entities.Identity
{
    [Owned]
    public class Address
    {
        public string Country { get; set; } = string.Empty;
        public string Government { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string Zipcode { get; set; } = string.Empty;
    }
}
