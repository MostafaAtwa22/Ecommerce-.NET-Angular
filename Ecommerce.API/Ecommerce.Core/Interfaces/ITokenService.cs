using Ecommerce.Core.Entities.Identity;
using Ecommerce.Core.googleDto;

namespace Ecommerce.Core.Interfaces
{
    public interface ITokenService
    {
        Task<string> CreateToken(ApplicationUser user);
        RefreshToken GenerateRefreshToken();
        void SetRefreshTokenInCookie(string refreshToken, DateTime expires);
        Task<ApplicationUser?> FindOrCreateUserByGoogleIdAsync(GoogleUserDto googleDto);
        Task CleanExpiredTokens();
    }
}