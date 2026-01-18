using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Ecommerce.API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string RetrieveEmailFromPrincipal(this ClaimsPrincipal user)
            => user?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value
               ?? user?.Claims?.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Email)?.Value
               ?? user?.Claims?.FirstOrDefault(x => x.Type == "email")?.Value!;

        public static string RetrieveUserIdFromPrincipal(this ClaimsPrincipal user)
            => user?.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.NameId)?.Value
               ?? user?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value
               ?? user?.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value
               ?? user?.Claims.FirstOrDefault(x => x.Type == "sub")?.Value!;
    }
}