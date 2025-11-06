using Ecommerce.Core.Entities.Identity;

namespace Ecommerce.Core.Interfaces
{
    public interface ITokenService
    {
        Task<string> CreateToken(ApplicationUser user);
    }
}