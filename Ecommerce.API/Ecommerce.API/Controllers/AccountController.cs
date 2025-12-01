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
        private readonly IMapper _mapper;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,
            IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
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

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, true);

            if (result.IsLockedOut)
                return BadRequest(new ApiResponse(400,
                    "Your account has been locked due to multiple failed attempts. Please try again later after 5 minutes."));

            if (!result.Succeeded)
                return BadRequest(new ApiResponse(400,
                    "Email Or password is worng, Try again!"));

            var response = _mapper.Map<UserDto>(user);
            response.Token = await _tokenService.CreateToken(user);

            return Ok(response);
        }

        [HttpPost("register")]
        [EnableRateLimiting("customer-register")] 
        public async Task<ActionResult<UserDto>> Register(RegisterDto dto)
        {
            var emailExists = await CheckEmailExistsAsync(dto.Email);
            if (emailExists.Value)
            {
                return new BadRequestObjectResult(new ApiValidationErrorResponse
                {
                    Errors = new[] { "Email address is in use" }
                });
            }

            if (!Enum.TryParse(dto.RoleName, true, out Role parsedRole))
                return BadRequest(new ApiResponse(400, "Invalid role specified."));

            var authCheck = ValidateRoleAuthorization(parsedRole);
            if (authCheck != null)
                return authCheck;

            var user = _mapper.Map<ApplicationUser>(dto);

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(new ApiResponse(400,
                    string.Join(", ", result.Errors.Select(e => e.Description))));

            var roleResult = await _userManager.AddToRoleAsync(user, parsedRole.ToString());
            if (!roleResult.Succeeded)
                return BadRequest(new ApiResponse(400,
                    string.Join(", ", roleResult.Errors.Select(e => e.Description))));

            var response = _mapper.Map<UserDto>(user);
            response.Token = await _tokenService.CreateToken(user);

            return Ok(response);
        }

        [HttpGet("emailexists/{email}")]
        [EnableRateLimiting("customer-browsing")]
        public async Task<ActionResult<bool>> CheckEmailExistsAsync([FromRoute] string email)
            => await _userManager.FindByEmailAsync(email) is not null;

        [HttpGet("usernameexists/{username}")]
        [EnableRateLimiting("customer-browsing")]
        public async Task<ActionResult<bool>> CheckUsernameExistsAsync([FromRoute] string username)
            => await _userManager.FindByNameAsync(username) is not null;

        private ActionResult<UserDto>? ValidateRoleAuthorization(Role targetRole)
        {
            if (targetRole == Role.Customer)
                return null;

            if (!User.Identity?.IsAuthenticated ?? true)
                return new UnauthorizedObjectResult(new ApiResponse(401,
                    "You must be logged in as SuperAdmin to assign Admin or SuperAdmin roles."));

            if (!User.IsInRole(Role.SuperAdmin.ToString()))
                return new ForbidResult();

            return null;
        }
    }
}