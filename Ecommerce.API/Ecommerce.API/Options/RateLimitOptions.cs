using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.API.Options
{
    public class RateLimitOptions
    {
        public const string SectionName = "RateLimiting";

        public AuthenticationOptions Authentication { get; set; } = new();
        public ProductsOptions Products { get; set; } = new();
        public AuthenticatedOptions Authenticated { get; set; } = new();
        public PaymentOptions Payment { get; set; } = new();
        public AdminOptions Admin { get; set; } = new();
        public GlobalOptions Global { get; set; } = new();
        public CustomerCartOptions CustomerCart { get; set; } = new();
        public CustomerOrdersOptions CustomerOrders { get; set; } = new();
        public CustomerProfileOptions CustomerProfile { get; set; } = new();
        public CustomerWishlistOptions CustomerWishlist { get; set; } = new();
        public CustomerRegisterOptions CustomerRegister { get; set; } = new();
    }
}