using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Core.Entities
{
    public class ProductBrand : BaseEntity
    {
        [Required, MinLength(3), MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}