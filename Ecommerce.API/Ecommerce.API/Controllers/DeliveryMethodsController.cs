using AutoMapper;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.API.Errors;
using Ecommerce.Core.Entities.orderAggregate;
using Ecommerce.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
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
        public async Task<ActionResult<IReadOnlyList<DeliveryMethodResponseDto>>> GetDeliveryMethods()
        {
            var deliveryMethods = await _unitOfWork.Repository<DeliveryMethod>().GetAllAsync();

            return Ok(_mapper.Map<IReadOnlyList<DeliveryMethodResponseDto>>(deliveryMethods));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DeliveryMethodResponseDto>> GetDeliveryMethod(int id)
        {
            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(id);

            if (deliveryMethod == null)
                return NotFound(new { message = "Delivery method not found" });

            return Ok(_mapper.Map<DeliveryMethodResponseDto>(deliveryMethod));
        }

        [HttpPost]
        public async Task<ActionResult<DeliveryMethodResponseDto>> CreateDeliveryMethod(DeliveryMethodDto createDto)
        {
            var deliveryMethod = _mapper.Map<DeliveryMethod>(createDto);

            await _unitOfWork.Repository<DeliveryMethod>().Create(deliveryMethod);
            await _unitOfWork.Complete();

            var returnDto = _mapper.Map<DeliveryMethodResponseDto>(deliveryMethod);

            return Ok(returnDto);
        }

        [HttpPut("{id}")]
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
