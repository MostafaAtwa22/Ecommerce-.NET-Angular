namespace Ecommerce.API.Controllers
{
    public class AdminsController : BaseApiController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public AdminsController(UserManager<ApplicationUser> userManager, 
            IMapper mapper) 
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        [HttpPost]
        [AuthorizePermission(Modules.Admin, CRUD.Create)]
        public async Task<ActionResult<string>> Create(AdminCreateDto dto)
        {
            var emailExists = await _userManager.FindByEmailAsync(dto.Email) is not null;
            if (emailExists)
                return BadRequest(new ApiResponse(400, "Email already in use"));

            if (!Enum.TryParse(dto.RoleName, true, out Role role))
                return BadRequest(new ApiResponse(400, "Invalid role specified"));

            var user = _mapper.Map<ApplicationUser>(dto);

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
                return BadRequest(new ApiResponse(400, "Failed to create user Admin: " 
                    + BuildErrors(createResult.Errors)));

            var roleResult = await _userManager.AddToRoleAsync(user, role.ToString());
            if (!roleResult.Succeeded)
                return BadRequest(new ApiResponse(400, "Failed to assign role to user Admin: " 
                    + BuildErrors(roleResult.Errors)));

            return Ok("Admin user created successfully");
        }

        private static string BuildErrors(IEnumerable<IdentityError> errors)
            => string.Join(", ", errors.Select(e => e.Description));
    }
}