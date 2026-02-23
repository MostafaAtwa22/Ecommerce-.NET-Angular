
namespace Ecommerce.API.Controllers
{
    public class RolesController : BaseApiController
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IPermissionService _permissionService;
        

        public RolesController(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            IPermissionService permissionService)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _mapper = mapper;
            _permissionService = permissionService;
        }

        [HttpGet]
        [AuthorizePermission(Modules.Roles, CRUD.Read)]
        [Cached(300)]
        public async Task<ActionResult<ICollection<RoleDto>>> GetAllRoles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var roleDtos = _mapper.Map<List<RoleDto>>(roles);

            foreach (var dto in roleDtos)
                dto.UserCount = (await _userManager.GetUsersInRoleAsync(dto.Name)).Count;

            return Ok(roleDtos);
        }

        [HttpGet("{id}")]
        [AuthorizePermission(Modules.Roles, CRUD.Read)]
        [Cached(300)]
        public async Task<ActionResult<RoleDto>> GetRoleById(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role is null)
                return NotFound(new ApiResponse(404, "Role not found"));

            var roleDto = _mapper.Map<RoleDto>(role);
            roleDto.UserCount = (await _userManager.GetUsersInRoleAsync(role.Name!)).Count;

            return Ok(roleDto);
        }

        [HttpPost]
        [AuthorizePermission(Modules.Roles, CRUD.Create)]
        [InvalidateCache("/api/roles")]
        public async Task<ActionResult<RoleDto>> Create(RoleToCreateDto dto)
        {
            var roleName = dto.Name.Trim();

            if (await _roleManager.RoleExistsAsync(roleName))
                return BadRequest(new ApiResponse(400, "Role already exists"));

            var role = _mapper.Map<IdentityRole>(dto);
            role.Name = roleName;

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
                return BadRequest(new ApiResponse(400, string.Join(", ", result.Errors.Select(e => e.Description))));

            return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, _mapper.Map<RoleDto>(role));
        }

        [HttpDelete("{id}")]
        [AuthorizePermission(Modules.Roles, CRUD.Delete)]
        [InvalidateCache("/api/roles")]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role is null)
                return NotFound(new ApiResponse(404, "Role not found"));

            if ((await _userManager.GetUsersInRoleAsync(role.Name!)).Any())
                return BadRequest(new ApiResponse(400, "Cannot delete role with assigned users"));

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
                return BadRequest(new ApiResponse(400,
                    string.Join(", ", result.Errors.Select(e => e.Description))));

            return NoContent();
        }

        [HttpGet("manage-user-roles/{userId}")]
        [AuthorizePermission(Modules.Roles, CRUD.Read)]
        [Cached(30)]
        public async Task<ActionResult<UserRolesDto>> GetManageUserRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound));

            var roles = await _roleManager.Roles.ToListAsync();
            var userRoleNames = await _userManager.GetRolesAsync(user);
            var userRoleNamesSet = userRoleNames.ToHashSet(StringComparer.OrdinalIgnoreCase);

            var checkBoxRoles = _mapper.Map<List<CheckBoxRoleManageDto>>(roles);
            checkBoxRoles.ForEach(r => r.IsSelected = userRoleNamesSet.Contains(r.RoleName));

            var userRolesDto = _mapper.Map<UserRolesDto>(user);
            userRolesDto.Roles = checkBoxRoles;

            return Ok(userRolesDto);
        }

        [HttpPut("update-role")]
        [AuthorizePermission(Modules.Roles, CRUD.Update)]
        [InvalidateCache("/api/roles")]
        public async Task<ActionResult<UserRolesDto>> UpdateRoles(UserRolesDto userRolesDto)
        {
            var user = await _userManager.FindByIdAsync(userRolesDto.UserId);
            if (user is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound));

            var currentUserRoles = await _userManager.GetRolesAsync(user);
            if (currentUserRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentUserRoles);
                if (!removeResult.Succeeded)
                    return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest));
            }

            var selectedRoles = userRolesDto.Roles
                .Where(r => r.IsSelected)
                .Select(r => r.RoleName)
                .ToList();

            if (selectedRoles.Any())
            {
                var addResult = await _userManager.AddToRolesAsync(user, selectedRoles);
                if (!addResult.Succeeded)
                    return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest));
            }
            return Ok(userRolesDto);
        }

        [HttpGet("manage-permissions/{id}")]
        [AuthorizePermission(Modules.Roles, CRUD.Read)]
        [Cached(30)]
        public async Task<ActionResult<RolePermissionsDto>> GetManagePermissions(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Role not found"));
    
            var roleClaimValues = await _permissionService.GetRolePermissionsAsync(role);
            var allPermissions = Permissions.GenerateAllPermissions();

            var permissionDtos = allPermissions
                .Select(permission => CreatePermissionCheckboxDto(permission, roleClaimValues.Contains(permission)))
                .ToList();

            var rolePermissionsDto = _mapper.Map<RolePermissionsDto>(role);
            rolePermissionsDto.Permissions = permissionDtos;

            return Ok(rolePermissionsDto);
        }

        [HttpPut("update-permissions/{id}")]
        [AuthorizePermission(Modules.Roles, CRUD.Update)]
        [InvalidateCache("/api/roles")]
        public async Task<ActionResult<RolePermissionsDto>> UpdatePermissions(string id, List<PermissionCheckboxDto> permissions)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role is null)
                return NotFound(new ApiResponse(404, "Role not found"));

            await _permissionService.RemoveAllPermissionsAsync(role);

            var selectedPermissions = permissions
                .Where(p => p.IsSelected)
                .Select(p => p.PermissionName);

            await _permissionService.AddPermissionsAsync(role, selectedPermissions);
            var rolePermissionsDto = _mapper.Map<RolePermissionsDto>(role);
            rolePermissionsDto.Permissions = permissions;

            return Ok(rolePermissionsDto);
        }

        private static PermissionCheckboxDto CreatePermissionCheckboxDto(string permission, bool isSelected)
        {
            var parts = permission.Split('.');
            return new PermissionCheckboxDto
            {
                PermissionName = permission,
                Module = parts.Length > 1 ? parts[1] : string.Empty,
                Action = parts.Length > 2 ? parts[2] : string.Empty,
                IsSelected = isSelected
            };
        }
    }
}
