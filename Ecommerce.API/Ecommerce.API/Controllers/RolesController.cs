using AutoMapper;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Errors;
using Ecommerce.Core.Entities.Identity;
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
    }
}