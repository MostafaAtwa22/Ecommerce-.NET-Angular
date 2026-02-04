using System.Net;
using AutoMapper;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.API.Errors;
using Ecommerce.Core.Constants;
using Ecommerce.Core.Entities.Emails;
using Ecommerce.Core.Entities.Identity;
using Ecommerce.Core.googleDto;
using Ecommerce.Core.Interfaces;
using Ecommerce.Infrastructure.Services;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IGoogleService _googleService;
        private readonly IMapper _mapper;
        private readonly IPermissionService _permissionService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,
            IEmailService emailService,
            IConfiguration config,
            IGoogleService googleService,
            IMapper mapper,
            IPermissionService permissionService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _emailService = emailService;
            _config = config;
            _googleService = googleService;
            _mapper = mapper;
            _permissionService = permissionService;
        }

        [HttpPost("login")]
        [EnableRateLimiting("customer-login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user is null)
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized));
            
            if (!await _userManager.IsEmailConfirmedAsync(user))
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest,
                    "Confirm your email first"));
        
            var lockMessage = GetLockMessage(user);
            if (!string.IsNullOrEmpty(lockMessage))
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, lockMessage));

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);

            if (result.IsLockedOut)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest,
                    "Your account has been locked due to multiple failed attempts. Try again later."));

            if (!result.Succeeded)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Email or password is wrong. Try again!"));

            if (user.TwoFactorEnabled)
            {
                await TwoFactorAuthReturn(user);
                return Ok(new LoginResponseDto { RequiresTwoFactor = true, Message = "Check your inbox for a verification code" });
            }

            var userDto = await CreateUserResponseAsync(user);
            return Ok(new LoginResponseDto { RequiresTwoFactor = false, Message = "Login successful", User = userDto });
        }

        [HttpPost("verify-2fa")]
        [EnableRateLimiting("customer-login")]
        public async Task<ActionResult<UserDto>> Verify2FA(Verify2FADto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user is null)
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized));

            if (!user.TwoFactorEnabled)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, 
                    "Two-factor authentication is not enabled for this account"));

            // Verify the 2FA token
            var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", dto.Code);
            
            if (!isValid)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, 
                    "Invalid or expired verification code"));

            var response = await CreateUserResponseAsync(user);
            return Ok(response);
        }

        [HttpPost("resend-2fa")]
        [EnableRateLimiting("customer-login")]
        public async Task<ActionResult<string>> Resend2FA([FromBody] ResendVerificationEmailDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Email is required"));

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "User not found"));

            if (!user.TwoFactorEnabled)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Two-factor authentication is not enabled"));

            await TwoFactorAuthReturn(user);
            return Ok("Check your inbox for a verification code");
        }
        
        [HttpPost("register")]
        [EnableRateLimiting("customer-register")]
        public async Task<ActionResult<string>> Register(RegisterDto dto)
        {
            if (await CheckEmailExistsAsync(dto.Email))
                return BadRequest(new ApiResponse(400, "Email already in use"));

            if (!Enum.TryParse(dto.RoleName, true, out Role role))
                return BadRequest(new ApiResponse(400, "Invalid role specified"));

            var roleAuthResult = ValidateRoleAuthorization(role);
            if (roleAuthResult != null)
                return roleAuthResult;

            var user = _mapper.Map<ApplicationUser>(dto);

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
                return BadRequest(new ApiResponse(400, BuildErrors(createResult.Errors)));

            var roleResult = await _userManager.AddToRoleAsync(user, role.ToString());
            if (!roleResult.Succeeded)
                return BadRequest(new ApiResponse(400, BuildErrors(roleResult.Errors)));

            await SendEmailVerificationAsync(user);

            return Ok("Please confirm your email. Check your inbox.");
        }
    
        [HttpPost("email-verification")]
        [EnableRateLimiting("customer-register")]
        public async Task<ActionResult<UserDto>> EmailVerfication([FromBody] EmailVerficationDto dto)
        {
            if (dto.Email is null || dto.Code is null)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest));
            
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound));
            
            var isVerified = await _userManager.ConfirmEmailAsync(user, dto.Code);

            if (!isVerified.Succeeded)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest));
            
            var response = await CreateUserResponseAsync(user);
            return Ok(response);
        }

        [HttpPost("resend-verification")] 
        [EnableRateLimiting("customer-register")]
        public async Task<IActionResult> ResendVerificationEmail(ResendVerificationEmailDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new ApiResponse(400, "Email is required"));

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user is null)
                return NotFound(new ApiResponse(404, "User not found"));

            if (user.EmailConfirmed)
                return BadRequest(new ApiResponse(400, "Email is already verified"));

            await SendEmailVerificationAsync(user);

            return Ok("Verification email resent successfully");
        }

        [HttpGet("refresh-token")]
        public async Task<ActionResult<UserDto>> RefreshToken()
        {
            var token = Request.Cookies["ecommerce_refreshToken"];
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new ApiResponse(401, "Refresh token missing"));

            var user = await _userManager.Users
                .Include(u => u.RefreshTokens)
                .SingleOrDefaultAsync(u => u.RefreshTokens!.Any(t => t.Token == token));

            if (user == null)
                return Unauthorized(new ApiResponse(401, "Invalid refresh token"));

            var storedToken = user.RefreshTokens!.Single(t => t.Token == token);

            if (!storedToken.IsActive)
                return Unauthorized(new ApiResponse(401, "Refresh token expired"));

            // Rotate refresh token
            storedToken.RevokedOn = DateTime.UtcNow;

            var newRefreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshTokens!.Add(newRefreshToken);

            await _userManager.UpdateAsync(user);

            _tokenService.SetRefreshTokenInCookie(
                newRefreshToken.Token,
                newRefreshToken.ExpiresOn);

            var response = _mapper.Map<UserDto>(user);
            response.Token = await _tokenService.CreateToken(user);
            response.RefreshTokenExpiration = newRefreshToken.ExpiresOn;

            return Ok(response);
        }

        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenDto dto)
        {
            var token = dto.Token ?? Request.Cookies["ecommerce_refreshToken"];

            if (string.IsNullOrEmpty(token))
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Token is required"));

            var user = await _userManager.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u =>
                    u.RefreshTokens!.Any(t => t.Token == token));

            if (user is null)
                return Ok("Token revoked");

            var refreshToken = user.RefreshTokens!.Single(t => t.Token == token);

            if (refreshToken.IsActive)
            {
                refreshToken.RevokedOn = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
            }

            Response.Cookies.Delete("ecommerce_refreshToken");

            return Ok("Token revoked");
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["ecommerce_refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return NoContent();

            var user = await _userManager.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u =>
                    u.RefreshTokens!.Any(t => t.Token == refreshToken));

            if (user == null)
                return NoContent();

            var token = user.RefreshTokens!.Single(t => t.Token == refreshToken);
            token.RevokedOn = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            Response.Cookies.Delete("ecommerce_refreshToken");
            return NoContent();
        }
        
        [HttpPost("google-login")]
        [EnableRateLimiting("customer-browsing")]
        public async Task<ActionResult<UserDto>> GoogleLogin([FromBody] GoogleLoginDto dto)
        {
            var validGoogleUser = await _googleService.ValidateGoogleToken(dto.IdToken);

            if (validGoogleUser is null)
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized));
            
            var user = await _tokenService.FindOrCreateUserByGoogleIdAsync(
                new GoogleUserDto
                {
                    Email = validGoogleUser.Email,
                    EmailConfirmed = validGoogleUser.EmailVerified,
                    FirstName = validGoogleUser.GivenName,
                    LastName = validGoogleUser.FamilyName,
                    GoogleId = validGoogleUser.JwtId,
                }
            );

            if (user is null)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest));

            var response = await CreateUserResponseAsync(user);
            return Ok(response);
        }

        [HttpGet("emailexists/{email}")]
        [EnableRateLimiting("customer-browsing")]
        public async Task<bool> CheckEmailExistsAsync(string email)
            => await _userManager.FindByEmailAsync(email) is not null;

        [HttpGet("usernameexists/{username}")]
        [EnableRateLimiting("customer-browsing")]
        public async Task<bool> CheckUsernameExistsAsync(string username)
            => await _userManager.FindByNameAsync(username) is not null;

        [HttpGet("permissions")]
        [Authorize]
        [EnableRateLimiting("customer-browsing")]
        public async Task<ActionResult<List<string>>> GetUserPermissions()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "User not authenticated"));

            var permissions = await _permissionService.GetUserPermissionsAsync(userId);
            return Ok(permissions);
        }

        [HttpPost("forgetpassword")]
        [EnableRateLimiting("customer-browsing")]
        public async Task<ActionResult<UserDto>> ForgetPassword(ForgetPasswordDto dto)
        {
            var (response, error) = await GenerateAndSendResetPasswordEmailAsync(dto.Email);
            if (error != null)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, error));

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
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest,
                    "No user with this email exists"));

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);

            if (!result.Succeeded)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest,
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

            var body = EmailTemplates.ResetPassword(resetLink);

            BackgroundJob.Enqueue<IEmailService>(x =>
                x.SendAsync(new EmailMessage
                {
                    To = user.Email!,
                    Subject = "Reset Your Password",
                    HtmlBody = body
                })
            );

            var response = _mapper.Map<UserDto>(user);
            response.Token = token;

            return (response, null);
        }

        private async Task<UserDto> CreateUserResponseAsync(ApplicationUser user)
        {
            var response = _mapper.Map<UserDto>(user);
            response.Token = await _tokenService.CreateToken(user);

            // Get an active refresh token or create a new one
            var refreshToken = user.RefreshTokens?.FirstOrDefault(t => t.IsActive)
                            ?? _tokenService.GenerateRefreshToken();

            // If it’s a new token, add it to the user and update
            if (!user.RefreshTokens!.Contains(refreshToken))
            {
                user.RefreshTokens.Add(refreshToken);
                await _userManager.UpdateAsync(user);
            }

            response.RefreshToken = refreshToken.Token;
            response.RefreshTokenExpiration = refreshToken.ExpiresOn;

            // Set refresh token in HTTP cookie
            _tokenService.SetRefreshTokenInCookie(refreshToken.Token, refreshToken.ExpiresOn);

            return response;
        }

        private static string BuildErrors(IEnumerable<IdentityError> errors)
            => string.Join(", ", errors.Select(e => e.Description));

        private ActionResult<string>? ValidateRoleAuthorization(Role role)
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

        private async Task SendEmailVerificationAsync(ApplicationUser user)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var confirmLink =
                $"{_config["UiUrl"]}/email-verification?email={user.Email}";

            var emailMessage = new EmailMessage
            {
                To = user.Email!,
                Subject = "Confirm your email",
                HtmlBody = EmailTemplates.ConfirmEmail(confirmLink, code)
            };
            BackgroundJob.Enqueue<IEmailService>(x => x.SendAsync(emailMessage));
        }

        private async Task TwoFactorAuthReturn(ApplicationUser user)
        {
            var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
            
            var emailMessage = new EmailMessage
            {
                To = user.Email!,
                Subject = "Your Two-Factor Authentication Code",
                HtmlBody = EmailTemplates.TwoFactorCode(token)
            };
            
            BackgroundJob.Enqueue<IEmailService>(x => x.SendAsync(emailMessage));
        }
    }
}
