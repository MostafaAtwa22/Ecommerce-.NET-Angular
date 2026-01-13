using System.Net;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.API.Errors;
using Ecommerce.Core.Constants;
using Ecommerce.Core.Entities.Identity;
using Ecommerce.Core.Interfaces;
using Ecommerce.Infrastructure.Services;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,
            IEmailService emailService,
            IConfiguration config,
            IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _emailService = emailService;
            _config = config;
            _mapper = mapper;
        }

        [HttpPost("login")]
        [EnableRateLimiting("customer-login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user is null)
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized));

            var lockMessage = GetLockMessage(user);
            if (!string.IsNullOrEmpty(lockMessage))
                return BadRequest(new ApiResponse(400, lockMessage));

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);

            if (result.IsLockedOut)
                return BadRequest(new ApiResponse(400,
                    "Your account has been locked due to multiple failed attempts. Try again later."));

            if (!result.Succeeded)
                return BadRequest(new ApiResponse(400, "Email or password is wrong. Try again!"));

            var response = await CreateUserResponseAsync(user);
            return Ok(response);
        }

        [HttpPost("googlelogin")]
        [EnableRateLimiting("customer-login")]
        public async Task<ActionResult<UserDto>> GoogleLogin([FromBody] GoogleLoginDto dto)
        {
            try
            {
                // Validate the Google ID token
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new string[]
                    {
                        _config["Authentication:Google:ClientId"]!
                    }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, settings);

                if (payload is null)
                    return BadRequest(new ApiResponse(400, "Invalid Google token"));

                // Check if user exists
                var user = await _userManager.FindByEmailAsync(payload.Email);

                if (user is null)
                {
                    // Create new user if they don't exist
                    user = new ApplicationUser
                    {
                        Email = payload.Email,
                        UserName = payload.Email,
                        FirstName = payload.GivenName ?? "",
                        LastName = payload.FamilyName ?? "",
                        EmailConfirmed = payload.EmailVerified,
                        ProfilePictureUrl = payload.Picture
                    };

                    var createResult = await _userManager.CreateAsync(user);

                    if (!createResult.Succeeded)
                        return BadRequest(new ApiResponse(400,
                            BuildErrors(createResult.Errors)));

                    // Add to Customer role by default
                    var roleResult = await _userManager.AddToRoleAsync(user, Role.Customer.ToString());

                    if (!roleResult.Succeeded)
                        return BadRequest(new ApiResponse(400,
                            BuildErrors(roleResult.Errors)));
                }
                else
                {
                    // Optionally update user profile picture if it changed
                    if (!string.IsNullOrEmpty(payload.Picture) &&
                        user.ProfilePictureUrl != payload.Picture)
                    {
                        user.ProfilePictureUrl = payload.Picture;
                        await _userManager.UpdateAsync(user);
                    }
                }

                // Check if account is locked
                if (await _userManager.IsLockedOutAsync(user))
                    return BadRequest(new ApiResponse(400,
                        "Your account is locked. Please try again later."));

                var response = await CreateUserResponseAsync(user);
                return Ok(response);
            }
            catch (InvalidJwtException)
            {
                return BadRequest(new ApiResponse(400, "Invalid Google token"));
            }
            catch
            {
                return BadRequest(new ApiResponse(400,
                    "Something went wrong during Google authentication. Please try again."));
            }
        }

        [HttpPost("register")]
        [EnableRateLimiting("customer-register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto dto)
        {
            if (await CheckEmailExistsAsync(dto.Email) == true)
                return BadRequest(new ApiResponse(400, "Email already in use"));

            if (!Enum.TryParse(dto.RoleName, true, out Role parsedRole))
                return BadRequest(new ApiResponse(400, "Invalid role specified."));

            var roleAuthResult = ValidateRoleAuthorization(parsedRole);
            if (roleAuthResult != null)
                return roleAuthResult;

            var user = _mapper.Map<ApplicationUser>(dto);

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
                return BadRequest(new ApiResponse(400,
                    BuildErrors(createResult.Errors)));

            var roleResult = await _userManager.AddToRoleAsync(user, parsedRole.ToString());
            if (!roleResult.Succeeded)
                return BadRequest(new ApiResponse(400,
                    BuildErrors(roleResult.Errors)));

            var response = await CreateUserResponseAsync(user);
            return Ok(response);
        }

        [HttpGet("refresh-token")]
        public async Task<ActionResult<UserDto>> GetRefreshToken()
        {
            //  Read refresh token from cookie
            var rawToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(rawToken))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "Refresh token missing"));

            //  Hash token
            var tokenHash = TokenService.HashToken(rawToken);

            //  Find user with this refresh token   
            var user = await _userManager.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u =>
                    u.RefreshTokens!.Any(t => t.TokenHash == tokenHash));

            if (user == null)
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "Invalid refresh token"));

            //  Get stored refresh token
            var storedToken = user.RefreshTokens!
                .Single(t => t.TokenHash == tokenHash);

            if (!storedToken.IsActive)
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "Refresh token expired or revoked"));

            //  ROTATE refresh token
            storedToken.RevokedOn = DateTime.UtcNow;

            var (newRawToken, newRefreshToken) =
                _tokenService.GenerateRefreshToken();

            storedToken.ReplacedByTokenHash = newRefreshToken.TokenHash;
            user.RefreshTokens!.Add(newRefreshToken);

            await _userManager.UpdateAsync(user);

            // Set new refresh token cookie
            _tokenService.SetRefreshTokenInCookie(
                newRawToken,
                newRefreshToken.ExpiresOn);

            // Return new access token
            return await CreateUserResponseAsync(user);
        }

        [HttpPost("revoke-token")]
        public async Task<ActionResult<string>> RevokeToken([FromBody] RevokeTokenDto dto)
        {
            var token = dto.Token ?? Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(token))
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Token is required"));

            token = WebUtility.UrlDecode(token).Trim();

            var tokenHash = TokenService.HashToken(token);

            // Find user who owns this refresh token
            var user = await _userManager.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.RefreshTokens!.Any(t => t.TokenHash == tokenHash));

            if (user == null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Token not found"));

            var refreshToken = user.RefreshTokens!.Single(t => t.TokenHash == tokenHash);

            if (!refreshToken.IsActive)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Token is already revoked or expired"));

            // Revoke the token
            refreshToken.RevokedOn = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            // If the token came from cookie, delete it
            if (dto.Token == null)
                Response.Cookies.Delete("refreshToken");

            return Ok("Revoke Success!");
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var rawToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(rawToken))
                return NoContent();

            var hash = TokenService.HashToken(rawToken);

            var user = await _userManager.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u =>
                    u.RefreshTokens!.Any(t => t.TokenHash == hash));

            if (user == null)
                return NoContent();

            var token = user.RefreshTokens!.Single(t => t.TokenHash == hash);
            token.RevokedOn = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            Response.Cookies.Delete("refreshToken");
            return NoContent();
        }
        
        [HttpGet("emailexists/{email}")]
        [EnableRateLimiting("customer-browsing")]
        public async Task<bool> CheckEmailExistsAsync(string email)
            => await _userManager.FindByEmailAsync(email) is not null;

        [HttpGet("usernameexists/{username}")]
        [EnableRateLimiting("customer-browsing")]
        public async Task<bool> CheckUsernameExistsAsync(string username)
            => await _userManager.FindByNameAsync(username) is not null;

        [HttpPost("forgetpassword")]
        [EnableRateLimiting("customer-browsing")]
        public async Task<ActionResult<UserDto>> ForgetPassword(ForgetPasswordDto dto)
        {
            var (response, error) = await GenerateAndSendResetPasswordEmailAsync(dto.Email);
            if (error != null)
                return BadRequest(new ApiResponse(400, error));

            return Ok(response);
        }

        [HttpPost("resend-resetpassword")]
        [EnableRateLimiting("customer-browsing")]
        public async Task<ActionResult> ResendResetPassword([FromForm] string email)
        {
            var (response, error) = await GenerateAndSendResetPasswordEmailAsync(email);

            if (error != null)
                return StatusCode(500, new ApiResponse(500, error));

            return Ok(response);
        }

        [HttpPost("resetpassword")]
        [EnableRateLimiting("customer-browsing")]
        public async Task<ActionResult<string>> ResetPassword(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user is null)
                return BadRequest(new ApiResponse(400,
                    "No user with this email exists"));

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);

            if (!result.Succeeded)
                return BadRequest(new ApiResponse(400,
                    BuildErrors(result.Errors)));

            return Ok("Reset password done successfully!");
        }

        // HELPERS
        private string? GetLockMessage(IdentityUser user, int defaultLockMinutes = 5)
        {
            if (user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                var remainingMinutes = (user.LockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes;

                if (remainingMinutes <= defaultLockMinutes)
                    return $"Your account is locked due to multiple failed attempts. Try again after {Math.Ceiling(remainingMinutes)} minutes.";

                return "Your account has been locked by an admin. Please contact support.";
            }

            return null; 
        }

        private async Task<(UserDto? User, string? ErrorMessage)> GenerateAndSendResetPasswordEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                return (null, "No user with this email exists");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            if (string.IsNullOrEmpty(token))
                return (null, "Failed to generate password reset token");

            var resetLink = $"{_config["UiUrl"]}/resetpassword?email={user.Email}&token={WebUtility.UrlEncode(token)}";

            var emailSent = await _emailService.SendResetPasswordEmailAsync(email, resetLink);
            if (!emailSent)
                return (null, "Failed to send reset password email");

            var response = _mapper.Map<UserDto>(user);
            response.Token = token;

            return (response, null);
        }

        private async Task<UserDto> CreateUserResponseAsync(ApplicationUser user)
        {
            var response = _mapper.Map<UserDto>(user);
            response.Token = await _tokenService.CreateToken(user);

            // Ensure RefreshTokens list exists
            user.RefreshTokens ??= new List<RefreshToken>();

            // Remove expired or revoked tokens
            foreach (var token in user.RefreshTokens.Where(t => !t.IsActive).ToList())
                user.RefreshTokens.Remove(token);

            // Get active refresh token or generate a new one
            var activeToken = user.RefreshTokens.FirstOrDefault(t => t.IsActive);
            if (activeToken is null)
            {
                var (rawToken, newRefreshToken) = _tokenService.GenerateRefreshToken();
                user.RefreshTokens.Add(newRefreshToken);
                await _userManager.UpdateAsync(user);

                _tokenService.SetRefreshTokenInCookie(rawToken, newRefreshToken.ExpiresOn);
                activeToken = newRefreshToken;
            }

            // Set refresh token expiration in DTO (for client info only)
            response.RefreshTokenExpiration = activeToken.ExpiresOn;

            return response;
        }

        private static string BuildErrors(IEnumerable<IdentityError> errors)
            => string.Join(", ", errors.Select(e => e.Description));

        private ActionResult<UserDto>? ValidateRoleAuthorization(Role role)
        {
            if (role == Role.Customer)
                return null;

            if (!User.Identity?.IsAuthenticated ?? true)
                return new UnauthorizedObjectResult(
                    new ApiResponse(StatusCodes.Status401Unauthorized,
                        "You must be logged in as SuperAdmin to assign Admin or SuperAdmin roles."));

            if (!User.IsInRole(Role.SuperAdmin.ToString()))
                return new ForbidResult();

            return null;
        }
    }
}
