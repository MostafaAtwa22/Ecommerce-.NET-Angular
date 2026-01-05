using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Core.Entities
{
    public class BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}