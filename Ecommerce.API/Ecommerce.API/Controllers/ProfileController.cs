using AutoMapper;
using Ecommerce.API.Dtos;
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

        [HttpGet("{email}")]
        [AllowAnonymous]
        public async Task<ActionResult<bool>> CheckEmailExistsAsync([FromRoute] string email)
            => await _userManager.FindByEmailAsync(email) is not null;

        [HttpGet("address")]
        public async Task<ActionResult<AddressDto>> GetAddress()
        {
            var user = await _userManager.FindUserByClaimsPrinciplesWithAddressAsync(HttpContext.User);

            return Ok(_mapper.Map<Address, AddressDto>(user?.Address!));
        }

        [HttpPut("address")]
        public async Task<ActionResult<AddressDto>> UpdateAddress(AddressDto dto)
        {
            var user = await _userManager.FindUserByClaimsPrinciplesWithAddressAsync(HttpContext.User);

            user!.Address = _mapper.Map<AddressDto, Address>(dto);

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(new ApiResponse(400,
                    string.Join(", ", result.Errors.Select(e => e.Description))));

            return Ok(_mapper.Map<Address, AddressDto>(user?.Address!));
        }
    }
}