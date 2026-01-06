using System.Security.Claims;
using AutoMapper;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Errors;
using Ecommerce.API.Helpers.Attributes;
using Ecommerce.Core.Entities.Identity;
using Ecommerce.Infrastructure.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class RolesController : BaseApiController
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public RolesController(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _mapper = mapper;
        }

        [HttpGet]
        [AuthorizePermission(Modules.Roles, CRUD.Read)]
        public async Task<ActionResult<ICollection<RoleDto>>> GetAllRoles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var roleUserCounts = new Dictionary<string, int>();

            foreach (var role in roles)
                roleUserCounts[role.Name!] =
                    (await _userManager.GetUsersInRoleAsync(role.Name!)).Count;

            var roleDtos = _mapper.Map<ICollection<RoleDto>>(roles);
            foreach (var dto in roleDtos)
                dto.UserCount = roleUserCounts[dto.Name];

            return Ok(roleDtos);
        }

        [HttpGet("{id}")]
        [AuthorizePermission(Modules.Roles, CRUD.Read)]
        public async Task<ActionResult<RoleDto>> GetRoleById(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role is null)
                return NotFound(new ApiResponse(404, "Role not found"));

            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            var userCount = usersInRole.Count;

            var roleDto = _mapper.Map<RoleDto>(role);
            roleDto.UserCount = userCount;

            return Ok(roleDto);
        }

        [HttpPost]
        [AuthorizePermission(Modules.Roles, CRUD.Create)]
        public async Task<ActionResult<RoleDto>> Create(RoleToCreateDto dto)
        {
            var normalizedRoleName = dto.Name?.Trim()!;

            if (await _roleManager.RoleExistsAsync(normalizedRoleName))
                return BadRequest(new ApiResponse(400, "Role name is already exists"));

            dto.Name = normalizedRoleName;
            var role = _mapper.Map<IdentityRole>(dto);

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
                return BadRequest(new ApiResponse(400,
                    string.Join(", ", result.Errors.Select(e => e.Description))));

            var roleDto = _mapper.Map<RoleDto>(role);

            return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, roleDto);
        }

        [HttpDelete("{id}")]
        [AuthorizePermission(Modules.Roles, CRUD.Delete)]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role is null)
                return NotFound(new ApiResponse(404, "Role not found"));

            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            if (usersInRole.Any())
                return BadRequest(new ApiResponse(400, "Cannot delete role with assigned users"));

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
                return BadRequest(new ApiResponse(400,
                    string.Join(", ", result.Errors.Select(e => e.Description))));

            return NoContent();
        }

        [HttpGet("manage-user-roles/{userId}")]
        [AuthorizePermission(Modules.Roles, CRUD.Read)]
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

        [HttpGet("manage-permissions/{Id}")]
        [AuthorizePermission(Modules.Roles, CRUD.Read)]
        public async Task<ActionResult<RolePermissionsDto>> GetManagePermissions(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            var roleClaimValues = await GetRolePermissionClaimsAsync(role!);
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
        public async Task<ActionResult<RolePermissionsDto>> UpdatePermissions(string id, List<PermissionCheckboxDto> permissions)
        {
            var role = await _roleManager.FindByIdAsync(id);
            await RemoveAllPermissionClaimsAsync(role!);

            var selectedPermissions = permissions
                .Where(p => p.IsSelected)
                .Select(p => p.PermissionName);

            await AddPermissionClaimsAsync(role!, selectedPermissions);

            var rolePermissionsDto = _mapper.Map<RolePermissionsDto>(role);
            rolePermissionsDto.Permissions = permissions;

            return Ok(rolePermissionsDto);
        }

        private async Task RemoveAllPermissionClaimsAsync(IdentityRole role)
        {
            var allClaims = await _roleManager.GetClaimsAsync(role);
            var permissionClaims = allClaims.Where(c => c.Type == Permissions.ClaimType);

            foreach (var claim in permissionClaims)
            {
                var result = await _roleManager.RemoveClaimAsync(role, claim);
                if (!result.Succeeded)
                    throw new Exception("Can't Remove the Permissions!");
            }
        }

        private async Task AddPermissionClaimsAsync(IdentityRole role, IEnumerable<string> permissions)
        {
            foreach (var permission in permissions)
            {
                var result = await _roleManager.AddClaimAsync(role, new Claim(Permissions.ClaimType, permission));
                if (!result.Succeeded)
                    throw new Exception("Can't Add the Permissions!");
            }
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

        private async Task<HashSet<string>> GetRolePermissionClaimsAsync(IdentityRole role)
        {
            var claims = await _roleManager.GetClaimsAsync(role);
            return claims
                .Where(c => c.Type == Permissions.ClaimType)
                .Select(c => c.Value)
                .ToHashSet();
        }
    }
}