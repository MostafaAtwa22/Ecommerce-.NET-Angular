using System.Net;
using AutoMapper;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.API.Errors;
using Ecommerce.API.Helpers.Attributes;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces;
using Ecommerce.Infrastructure.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Ecommerce.API.Controllers
{
    [EnableRateLimiting("customer-browsing")]
    public class ProductTypesController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductTypesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [Cached(600)]
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ProductBrandAndTypeResponseDto>>> GetAll()
        {
            var types = await _unitOfWork.Repository<ProductType>().GetAllAsync();
            return Ok(_mapper.Map<IReadOnlyList<ProductBrandAndTypeResponseDto>>(types));
        }

        [Cached(600)]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductBrandAndTypeResponseDto>> GetById(int id)
        {
            var type = await _unitOfWork.Repository<ProductType>().GetByIdAsync(id);
            if (type is null)
                return NotFound(new ApiResponse((int)HttpStatusCode.NotFound));

            return Ok(_mapper.Map<ProductBrandAndTypeResponseDto>(type));
        }

        [HttpPost]
        [DisableRateLimiting]
        [AuthorizePermission(Modules.Roles, CRUD.Create)]
        public async Task<ActionResult<ProductBrandAndTypeResponseDto>> Create(ProductBrandAndTypeCreationDto creationDto)
        {
            var type = _mapper.Map<ProductBrandAndTypeCreationDto, ProductType>(creationDto);
            await _unitOfWork.Repository<ProductType>().Create(type);
            await _unitOfWork.Complete();

            return CreatedAtAction(nameof(GetById), new { id = type.Id },
                _mapper.Map<ProductBrandAndTypeResponseDto>(type));
        }

        [HttpDelete("{id:int}")]
        [DisableRateLimiting]
        [AuthorizePermission(Modules.Roles, CRUD.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var type = await _unitOfWork.Repository<ProductType>().GetByIdAsync(id);
            if (type is null)
                return NotFound(new ApiResponse((int)HttpStatusCode.NotFound));

            _unitOfWork.Repository<ProductType>().Delete(type);
            await _unitOfWork.Complete();

            return NoContent();
        }
    }
}
