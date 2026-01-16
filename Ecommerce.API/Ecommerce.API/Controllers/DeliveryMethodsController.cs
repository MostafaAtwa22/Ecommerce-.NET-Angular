using AutoMapper;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.API.Errors;
using Ecommerce.API.Helpers.Attributes;
using Ecommerce.Core.Entities.orderAggregate;
using Ecommerce.Core.Interfaces;
using Ecommerce.Infrastructure.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Ecommerce.API.Controllers
{
    [EnableRateLimiting("customer-browsing")]
    public class DeliveryMethodsController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DeliveryMethodsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [AuthorizePermission(Modules.DeliveryMethods, CRUD.Read)]
        public async Task<ActionResult<IReadOnlyList<DeliveryMethodResponseDto>>> GetDeliveryMethods()
        {
            var deliveryMethods = (await _unitOfWork.Repository<DeliveryMethod>().GetAllAsync())
                .OrderByDescending(x => x.Price)
                .ToList();

            return Ok(_mapper.Map<IReadOnlyList<DeliveryMethodResponseDto>>(deliveryMethods));
        }

        [HttpGet("{id}")]
        [AuthorizePermission(Modules.DeliveryMethods, CRUD.Read)]
        public async Task<ActionResult<DeliveryMethodResponseDto>> GetDeliveryMethod(int id)
        {
            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(id);

            if (deliveryMethod == null)
                return NotFound(new ApiResponse(404, "Delivery Method not found"));

            return Ok(_mapper.Map<DeliveryMethodResponseDto>(deliveryMethod));
        }

        [HttpPost]
        [DisableRateLimiting]
        [AuthorizePermission(Modules.DeliveryMethods, CRUD.Create)]
        public async Task<ActionResult<DeliveryMethodResponseDto>> CreateDeliveryMethod(DeliveryMethodDto createDto)
        {
            var deliveryMethod = _mapper.Map<DeliveryMethod>(createDto);

            await _unitOfWork.Repository<DeliveryMethod>().Create(deliveryMethod);
            await _unitOfWork.Complete();

            var returnDto = _mapper.Map<DeliveryMethodResponseDto>(deliveryMethod);

            return CreatedAtAction(nameof(GetDeliveryMethod),
                new { id = deliveryMethod.Id },
                returnDto);
        }

        [HttpPut("{id}")]
        [DisableRateLimiting]
        [AuthorizePermission(Modules.DeliveryMethods, CRUD.Update)]
        public async Task<IActionResult> UpdateDeliveryMethod(int id, DeliveryMethodDto updateDto)
        {
            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(id);

            if (deliveryMethod == null)
                return NotFound(new ApiResponse(400));

            _mapper.Map(updateDto, deliveryMethod); 

            _unitOfWork.Repository<DeliveryMethod>().Update(deliveryMethod);
            await _unitOfWork.Complete();

            return Ok(_mapper.Map<DeliveryMethodResponseDto>(deliveryMethod));
        }

        [HttpDelete("{id}")]
        [DisableRateLimiting]
        [AuthorizePermission(Modules.DeliveryMethods, CRUD.Delete)]
        public async Task<IActionResult> DeleteDeliveryMethod(int id)
        {
            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(id);

            if (deliveryMethod == null)
                return NotFound(new ApiResponse(400));

            _unitOfWork.Repository<DeliveryMethod>().Delete(deliveryMethod);
            await _unitOfWork.Complete();

            return Ok(_mapper.Map<DeliveryMethodResponseDto>(deliveryMethod));
        }
    }
}
