using AutoMapper;
using Ecommerce.API.Dtos;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.API.Errors;
using Ecommerce.API.Extensions;
using Ecommerce.Core.Constants;
using Ecommerce.Core.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [Authorize]
    public class ProfilesController : BaseApiController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public ProfilesController(UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        [HttpGet("profile")]
        public async Task<ActionResult<ProfileResponseDto>> GetProfile()
        {
            var user = await _userManager.FindUserByClaimPrinciplesAsync(HttpContext.User);

            return Ok(_mapper.Map<ProfileResponseDto>(user));
        }

        [HttpGet("address")]
        public async Task<ActionResult<AddressDto>> GetAddress()
        {
            var user = await _userManager.FindUserByClaimsPrinciplesWithAddressAsync(HttpContext.User);

            return Ok(_mapper.Map<Address, AddressDto>(user?.Address!));
        }

        [HttpPut("address")]
        public async Task<ActionResult<AddressDto>> UpdateAddress([FromBody]AddressDto dto)
        {
            var user = await _userManager.FindUserByClaimsPrinciplesWithAddressAsync(HttpContext.User);

            user!.Address = _mapper.Map<AddressDto, Address>(dto);

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(new ApiResponse(400,
                    string.Join(", ", result.Errors.Select(e => e.Description))));

            return Ok(_mapper.Map<Address, AddressDto>(user?.Address!));
        }

        [HttpPost("changePassword")]
        public async Task<ActionResult<bool>> ChangePassword([FromBody]ChangePassowrdDto dto)
        {
            var user = await _userManager.FindUserByClaimPrinciplesAsync(HttpContext.User);

            if (user is null)
                return NotFound(new ApiResponse(404));

            var result = await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);

            if (!result.Succeeded)
                return BadRequest(new ApiResponse(400,
                    string.Join(", ", result.Errors.Select(e => e.Description))));

            return Ok(true);
        }

        [HttpPost("setPassword")]
        public async Task<ActionResult<bool>> SetPassword([FromBody]SetPasswordDto dto)
        {
            var user = await _userManager.FindUserByClaimPrinciplesAsync(HttpContext.User);

            if (user is null)
                return NotFound(new ApiResponse(404));

            var result = await _userManager.AddPasswordAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(new ApiResponse(400,
                    string.Join(", ", result.Errors.Select(e => e.Description))));

            return Ok(true);
        }
        
        [HttpDelete("deleteProfile")]
        public async Task<ActionResult<bool>> DeleteProfile ([FromBody]DeleteProfileDto dto)
        {
            var user = await _userManager.FindUserByClaimPrinciplesAsync(HttpContext.User);

            if (user is null)
                return NotFound(new ApiResponse(404));

            if (!await _userManager.CheckPasswordAsync(user, dto.Password))
                return BadRequest(new ApiResponse(400, "Incorrect password"));

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                return BadRequest(new ApiResponse(400,
                    string.Join(", ", result.Errors.Select(e => e.Description))));

            return Ok(true);
        }
    }
}