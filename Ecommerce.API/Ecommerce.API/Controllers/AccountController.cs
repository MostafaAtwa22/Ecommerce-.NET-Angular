using System.Net;
using AutoMapper;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.API.Errors;
using Ecommerce.Core.Constants;
using Ecommerce.Core.Entities.Identity;
using Ecommerce.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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
                return Unauthorized(new ApiResponse(401));

            if (await _userManager.IsLockedOutAsync(user))
                return BadRequest(new ApiResponse(400,
                    "Your account is locked. Please try again later after 5 minutes."));

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);

            if (result.IsLockedOut)
                return BadRequest(new ApiResponse(400,
                    "Your account has been locked due to multiple failed attempts. Please try again later after 5 minutes."));

            if (!result.Succeeded)
                return BadRequest(new ApiResponse(400,
                    "Email or password is wrong. Try again!"));

            var response = await CreateUserResponseAsync(user);
            return Ok(response);
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

        // CHECK EMAIL / USERNAME
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
                    new ApiResponse(401,
                        "You must be logged in as SuperAdmin to assign Admin or SuperAdmin roles."));

            if (!User.IsInRole(Role.SuperAdmin.ToString()))
                return new ForbidResult();

            return null;
        }
    }
}
