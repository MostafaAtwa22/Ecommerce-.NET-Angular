using AutoMapper;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.API.Errors;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [Authorize]
    [ApiController]
    public class CouponsController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICouponService _couponService;

        public CouponsController(IUnitOfWork unitOfWork, IMapper mapper, ICouponService couponService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _couponService = couponService;
        }

        [HttpPost]
        [AuthorizePermission(Modules.Coupons, CRUD.Create)]
        public async Task<ActionResult<CouponResponseDto>> Create([FromBody] CouponCreateDto dto)
        {
            var coupon = _mapper.Map<Coupon>(dto);
            coupon.Code = coupon.Code.ToUpperInvariant(); 

            await _unitOfWork.Repository<Coupon>().Create(coupon);
            await _unitOfWork.Complete();

            return Ok(_mapper.Map<CouponResponseDto>(coupon));
        }

        [HttpGet]
        [AuthorizePermission(Modules.Coupons, CRUD.Read)]
        public async Task<ActionResult<IReadOnlyList<CouponResponseDto>>> GetAll()
        {
            var coupons = await _unitOfWork.Repository<Coupon>().GetAllAsync();
            return Ok(_mapper.Map<IReadOnlyList<CouponResponseDto>>(coupons));
        }

        [HttpDelete("{id}")]
        [AuthorizePermission(Modules.Coupons, CRUD.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var coupon = await _unitOfWork.Repository<Coupon>().GetByIdAsync(id);
            if (coupon is null) return NotFound(new ApiResponse(StatusCodes.Status404NotFound));

            _unitOfWork.Repository<Coupon>().Delete(coupon);
            await _unitOfWork.Complete();

            return NoContent();
        }

        [HttpPost("validate")]
        [Authorize]
        public async Task<ActionResult<CouponResponseDto>> Validate([FromQuery] string code)
        {
            var coupon = await _couponService.GetValidCouponAsync(code.ToUpperInvariant());

            if (coupon is null)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Invalid or expired coupon code"));

            return Ok(_mapper.Map<CouponResponseDto>(coupon));
        }
    }
}
