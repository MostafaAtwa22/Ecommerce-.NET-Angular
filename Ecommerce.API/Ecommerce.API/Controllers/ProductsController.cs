using System.Net;
using AutoMapper;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.API.Errors;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces;
using Ecommerce.Core.Params;
using Ecommerce.Core.Spec;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    public class ProductsController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductsController(IUnitOfWork unitOfWork,
        IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ProductResponseDto>>> GetAll([FromQuery] ProductSpecParams specParams)
        {
            var spec = new ProductWithTypeAndBrandSpec(specParams);
            var products = await _unitOfWork.Repository<Product>()
                .GetAllWithSpecAsync(spec);

            var data = _mapper.Map<IReadOnlyList<Product>, IReadOnlyList<ProductResponseDto>>(products);
            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductResponseDto>> GetById(int id)
        {
            var spec = new ProductWithTypeAndBrandSpec(id);
            var product = await _unitOfWork.Repository<Product>()
                .GetWithSpecAsync(spec);

            if (product is null)
                return NotFound(new ApiResponse((int)HttpStatusCode.NotFound));

            var productDto = _mapper.Map<Product, ProductResponseDto>(product);
            return Ok(productDto);
        }

        [HttpPost]
        public async Task<ActionResult<ProductResponseDto>> Create(ProductCreationDto creationDto)
        {
            var product = _mapper.Map<ProductCreationDto, Product>(creationDto);

            await _unitOfWork.Repository<Product>().Create(product);

            await _unitOfWork.Complete();

            var spec = new ProductWithTypeAndBrandSpec(product.Id);
            var createdProduct = await _unitOfWork.Repository<Product>()
                .GetWithSpecAsync(spec);

            return Ok(_mapper.Map<Product, ProductResponseDto>(createdProduct!));
        }

        [HttpPut]
        public async Task<ActionResult<ProductResponseDto>> Update(ProductUpdateDto updateDto)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(updateDto.ProductId);

            if (product is null)
                return NotFound(new ApiResponse((int)HttpStatusCode.NotFound));

            _mapper.Map(updateDto, product);
            
            _unitOfWork.Repository<Product>().Update(product);
            await _unitOfWork.Complete();

            var spec = new ProductWithTypeAndBrandSpec(product.Id);
            var updatedProduct = await _unitOfWork.Repository<Product>()
                .GetWithSpecAsync(spec);

            return Ok(_mapper.Map<Product, ProductResponseDto>(updatedProduct!));
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ProductResponseDto>> Delete(int id)
        {            
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);

            if (product is null)
                return NotFound(new ApiResponse((int)HttpStatusCode.NotFound));

            _unitOfWork.Repository<Product>().Delete(product);
            await _unitOfWork.Complete();

            var spec = new ProductWithTypeAndBrandSpec(product.Id);
            var deletedProduct = await _unitOfWork.Repository<Product>()
                .GetWithSpecAsync(spec);

            return Ok(_mapper.Map<Product, ProductResponseDto>(deletedProduct!));
        }
    }
}