using System.Net;
using AutoMapper;
using Ecommerce.API.Dtos;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.API.Errors;
using Ecommerce.API.Extensions;
using Ecommerce.API.Helpers.Attributes;
using Ecommerce.Core.Entities.Identity;
using Ecommerce.Core.Interfaces;
using Ecommerce.Core.Params;
using Ecommerce.Core.Spec;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Controllers
{
    [Authorize]
    [EnableRateLimiting("customer-profile")]
    public class ProfilesController : BaseApiController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileService _fileService;
        private readonly IMapper _mapper;

        public ProfilesController(UserManager<ApplicationUser> userManager,
            IFileService fileService,
            IMapper mapper)
        {
            _userManager = userManager;
            _fileService = fileService;
            _mapper = mapper;
        }

        [Cached(600)]
        [HttpGet("users")]
        [Authorize(Roles = "Admin, SuperAdmin")]
        public async Task<ActionResult<Pagination<ProfileResponseDto>>> GetAllUsers(
            [FromQuery] UserSpecParams specParams)
        {
            var baseQuery = _userManager.Users.AsQueryable();

            var countQuery = await IdentityUserSpecification
                .ApplyAsync(baseQuery, specParams, _userManager, forCount: true);

            var totalItems = await countQuery.CountAsync();

            var usersQuery = await IdentityUserSpecification
                .ApplyAsync(baseQuery, specParams, _userManager);

            var users = await usersQuery.ToListAsync();

            var data = _mapper.Map<IReadOnlyList<ProfileResponseDto>>(users);

            return Ok(new Pagination<ProfileResponseDto>(
                specParams.PageIndex,
                specParams.PageSize,
                totalItems,
                data
            ));
        }

        [HttpGet("profile")]
        [Cached(500)]
        public async Task<ActionResult<ProfileResponseDto>> GetProfile()
        {
            var user = await _userManager.FindUserByClaimPrinciplesAsync(HttpContext.User);

            return Ok(_mapper.Map<ProfileResponseDto>(user));
        }

        [HttpPatch("profile/json")]
        public async Task<ActionResult<ProfileResponseDto>> UpdateProfileJsonPatch(
            [FromBody] JsonPatchDocument<ProfileUpdateDto> patchDoc)
        {
            if (patchDoc == null)
                return BadRequest(new ApiResponse(400, "Invalid patch document"));

            var user = await _userManager.FindUserByClaimPrinciplesAsync(HttpContext.User);

            if (user == null)
                return NotFound(new ApiResponse(404));

            var profileDto = _mapper.Map<ProfileUpdateDto>(user);

            patchDoc.ApplyTo(profileDto, ModelState);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _mapper.Map(profileDto, user);

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(new ApiResponse(400,
                    string.Join(", ", result.Errors.Select(e => e.Description))));

            return Ok(_mapper.Map<ProfileResponseDto>(user));
        }

        [HttpPatch("profile/image")]
        public async Task<ActionResult<ProfileResponseDto>> UpdateProfileImage([FromForm] ProfileImageUpdateDto dto)
        {
            if (dto.ProfileImageFile is null)
                return BadRequest(new ApiResponse(400, "No file provided"));

            var user = await _userManager.FindUserByClaimPrinciplesAsync(HttpContext.User);
            if (user == null)
                return NotFound(new ApiResponse(404));

            var oldImage = user.ProfilePictureUrl;
            user.ProfilePictureUrl = await _fileService.SaveFileAsync(dto.ProfileImageFile, "users");

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
                    _fileService.DeleteFile(user.ProfilePictureUrl);

                return BadRequest(new ApiResponse(400,
                    string.Join(", ", result.Errors.Select(e => e.Description))));
            }

            if (!string.IsNullOrEmpty(oldImage))
                _fileService.DeleteFile(oldImage);

            return Ok(_mapper.Map<ProfileResponseDto>(user));
        }

        [HttpDelete("profile/image")]
        public async Task<ActionResult<ProfileResponseDto>> DeleteProfileImage()
        {
            var user = await _userManager.FindUserByClaimPrinciplesAsync(HttpContext.User);
            if (user is null)
                return NotFound(new ApiResponse(404));

            if (string.IsNullOrEmpty(user.ProfilePictureUrl))
                return BadRequest(new ApiResponse(400, "User has no profile image."));

            var oldImage = user.ProfilePictureUrl;

            user.ProfilePictureUrl = null!;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(new ApiResponse(400,
                    string.Join(", ", result.Errors.Select(e => e.Description))));

            if (!string.IsNullOrEmpty(oldImage))
                _fileService.DeleteFile(oldImage);

            return Ok(_mapper.Map<ProfileResponseDto>(user));
        }

        [HttpGet("address")]
        [Cached(500)]
        public async Task<ActionResult<AddressDto>> GetAddress()
        {
            var user = await _userManager.FindUserByClaimsPrinciplesWithAddressAsync(HttpContext.User);

            return Ok(_mapper.Map<Address, AddressDto>(user?.Address!));
        }
        
        [HttpPost("lock/{id}")]
        public async Task<ActionResult<ProfileResponseDto>> LockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound));
            
            if (!user.LockoutEnabled)
            {
                user.LockoutEnabled = true;
                var updateResult = await _userManager.UpdateAsync(user);
                if(!updateResult.Succeeded)
                    return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Failed to enable lockout for user."));
            }
            var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMonths(1));
            if (!result.Succeeded)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest));
            
            return Ok(_mapper.Map<ProfileResponseDto>(user));
        }

        [HttpPost("unlock/{id}")]
        public async Task<ActionResult<ProfileResponseDto>> UnlockUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound));
            
            var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
            if (!result.Succeeded)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest));
            
            return Ok(_mapper.Map<ProfileResponseDto>(user));
        }

        [HttpPut("address")]
        public async Task<ActionResult<AddressDto>> UpdateAddress([FromBody] AddressDto dto)
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
        public async Task<ActionResult<bool>> ChangePassword([FromBody] ChangePassowrdDto dto)
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
        public async Task<ActionResult<bool>> SetPassword([FromBody] SetPasswordDto dto)
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
        public async Task<ActionResult<bool>> DeleteProfile([FromBody] DeleteProfileDto dto)
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

            if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
                _fileService.DeleteFile(user.ProfilePictureUrl);
            return Ok(true);
        }
    }
}