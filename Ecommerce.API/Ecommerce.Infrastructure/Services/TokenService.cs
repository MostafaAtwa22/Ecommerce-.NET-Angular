using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using AutoMapper;

namespace Ecommerce.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;


        public TokenService(IConfiguration config, 
        UserManager<ApplicationUser> userManager, 
        RoleManager<IdentityRole> roleManager,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
        {
            _config = config;
            _userManager = userManager;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Token:Key"]!));
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }

        public async Task<string> CreateToken(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();

            foreach (var role in userRoles)
                roleClaims.Add(new Claim("roles", role));

            // Removed permission claims to reduce token size and enforce dynamic checking
            // Permissions are now checked via IPermissionService (backed by Redis)

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

        public async Task<ApplicationUser?> FindOrCreateUserByGoogleIdAsync(GoogleUserDto googleDto)
        {
            var user = await _userManager.FindByEmailAsync(googleDto.Email);

            if (user is not null)
            {
                if (string.IsNullOrEmpty(user.GoogleId))
                {
                    user.GoogleId = googleDto.GoogleId;
                    await _userManager.UpdateAsync(user);
                }
                return user;
            }

            var newUser = _mapper.Map<ApplicationUser>(googleDto);

            var result = await _userManager.CreateAsync(newUser);

            if (!result.Succeeded)
                return null;

            await _userManager.AddToRoleAsync(newUser, Role.Customer.ToString());

            return newUser;
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
                .Append("ecommerce_refreshToken", rawToken, options);
        }
    
        public async Task CleanExpiredTokens()
        {
            var users = _userManager.Users
                .Include(u => u.RefreshTokens)
                .ToList();

            foreach (var user in users)
            {
                var expired = user.RefreshTokens!
                    .Where(t => !t.IsActive)
                    .ToList();

                if (expired.Any())
                {
                    foreach (var token in expired)
                        user.RefreshTokens!.Remove(token);

                    await _userManager.UpdateAsync(user);
                }
            }
        }
    }
}
