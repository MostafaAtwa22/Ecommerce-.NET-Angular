using System.Net;
using AutoMapper;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.API.Errors;
using Ecommerce.API.Helpers.Attributes;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    public class ProductBrandsController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductBrandsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [Cached(600)]
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ProductBrandAndTypeResponseDto>>> GetAll()
        {
            var brands = await _unitOfWork.Repository<ProductBrand>().GetAllAsync();
            return Ok(_mapper.Map<IReadOnlyList<ProductBrandAndTypeResponseDto>>(brands));
        }

        [Cached(600)]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductBrandAndTypeResponseDto>> GetById(int id)
        {
            var brand = await _unitOfWork.Repository<ProductBrand>().GetByIdAsync(id);
            if (brand is null)
                return NotFound(new ApiResponse((int)HttpStatusCode.NotFound));

            return Ok(_mapper.Map<ProductBrandAndTypeResponseDto>(brand));
        }

        [HttpPost]
        public async Task<ActionResult<ProductBrandAndTypeResponseDto>> Create(ProductBrandAndTypeCreationDto creationDto)
        {
            var brand = _mapper.Map<ProductBrandAndTypeCreationDto, ProductBrand>(creationDto);
            await _unitOfWork.Repository<ProductBrand>().Create(brand);
            await _unitOfWork.Complete();

            return CreatedAtAction(nameof(GetById), new { id = brand.Id },
                _mapper.Map<ProductBrandAndTypeResponseDto>(brand));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var brand = await _unitOfWork.Repository<ProductBrand>().GetByIdAsync(id);
            if (brand is null)
                return NotFound(new ApiResponse((int)HttpStatusCode.NotFound));

            _unitOfWork.Repository<ProductBrand>().Delete(brand);
            await _unitOfWork.Complete();

            return NoContent();
        }
    }
}
