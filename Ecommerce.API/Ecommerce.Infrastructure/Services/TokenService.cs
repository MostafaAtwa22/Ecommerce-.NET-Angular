using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Azure;
using Ecommerce.Core.Entities.Identity;
using Ecommerce.Core.Interfaces;
using Ecommerce.Infrastructure.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Ecommerce.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public TokenService(IConfiguration config, 
        UserManager<ApplicationUser> userManager, 
        RoleManager<IdentityRole> roleManager,
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor)
        {
            _config = config;
            _userManager = userManager;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Token:Key"]!));
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> CreateToken(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();

            foreach (var role in userRoles)
                roleClaims.Add(new Claim("roles", role));

            // Get all role permissions
            var rolePermissions = new List<Claim>();
            foreach (var roleName in userRoles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    var allClaims = await _roleManager.GetClaimsAsync(role);
                    var permissionClaims = allClaims
                        .Where(c => c.Type == Permissions.ClaimType)
                        .Select(c => new Claim(Permissions.ClaimType, c.Value));

                    rolePermissions.AddRange(permissionClaims);
                }
            }

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.NameId, user.Id!),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("is_lockout", user.LockoutEnabled.ToString().ToLower())
            };

            // Combine all claims
            claims.AddRange(userClaims);
            claims.AddRange(roleClaims);
            claims.AddRange(rolePermissions);

            // Add standard Role claims for Identity
            claims.AddRange(userRoles.Select(r => new Claim(ClaimTypes.Role, r)));

            // Signing credentials
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            // Token descriptor
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Audience = _config["Token:Audience"],
                Issuer = _config["Token:Issuer"],
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_config["Token:LifetimeInMinutes"]!)),
                SigningCredentials = creds
            };

            // Create token
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            return jwtToken;
        }

        public RefreshToken GenerateRefreshToken()
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);

            return new RefreshToken
            {
                Token = Convert.ToBase64String(bytes),
                CreatedOn = DateTime.UtcNow,
                ExpiresOn = DateTime.UtcNow.AddDays(30)
            };
        }

        public void SetRefreshTokenInCookie(string rawToken, DateTime expires)
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Expires = expires.ToLocalTime(),
                Secure = true, 
                SameSite = SameSiteMode.None, 
            };

            _httpContextAccessor.HttpContext!
                .Response
                .Cookies
                .Append("refreshToken", rawToken, options);
        }
    }
}
