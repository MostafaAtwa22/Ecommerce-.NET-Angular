using System.Net;
using AutoMapper;
using Ecommerce.API.Dtos;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.API.Errors;
using Ecommerce.API.Helpers.Attributes;
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
        private readonly IFileService _fileService;
        private readonly IMapper _mapper;

        public ProductsController(IUnitOfWork unitOfWork,
            IFileService fileService,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _mapper = mapper;
        }

        [Cached(600)]
        [HttpGet]
        public async Task<ActionResult<Pagination<ProductResponseDto>>> GetAll([FromQuery] ProductSpecParams specParams)
        {
            var spec = new ProductWithTypeAndBrandSpec(specParams, forCount: false);
            var countSpec = new ProductWithTypeAndBrandSpec(specParams, forCount: true);

            var totalItems = await _unitOfWork.Repository<Product>()
                .CountAsync(countSpec);
            var products = await _unitOfWork.Repository<Product>()
                .GetAllWithSpecAsync(spec);


            var data = _mapper.Map<IReadOnlyList<Product>, IReadOnlyList<ProductResponseDto>>(products);

            return Ok(new Pagination<ProductResponseDto>(specParams.PageIndex,
                specParams.PageSize,
                totalItems,
                data));
        }

        [Cached(600)]
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductResponseDto>> GetById([FromQuery] int id)
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
        public async Task<ActionResult<ProductResponseDto>> Create([FromForm] ProductCreationDto creationDto)
        {
            var product = _mapper.Map<ProductCreationDto, Product>(creationDto);

            product.PictureUrl = await _fileService.SaveFileAsync(creationDto.ImageFile, "products");

            await _unitOfWork.Repository<Product>().Create(product);

            await _unitOfWork.Complete();

            var spec = new ProductWithTypeAndBrandSpec(product.Id);
            var createdProduct = await _unitOfWork.Repository<Product>()
                .GetWithSpecAsync(spec);

            return Ok(_mapper.Map<Product, ProductResponseDto>(createdProduct!));
        }

        [HttpPut]
        public async Task<ActionResult<ProductResponseDto>> Update([FromForm] ProductUpdateDto updateDto)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(updateDto.ProductId);

            if (product is null)
                return NotFound(new ApiResponse((int)HttpStatusCode.NotFound));

            var hasNewImage = updateDto.ImageFile is not null;
            var oldImage = product.PictureUrl;

            _mapper.Map(updateDto, product);

            if (hasNewImage)
                product.PictureUrl = await _fileService.SaveFileAsync(updateDto.ImageFile!, "products");

            _unitOfWork.Repository<Product>().Update(product);
            await _unitOfWork.Complete();

            var spec = new ProductWithTypeAndBrandSpec(product.Id);
            var updatedProduct = await _unitOfWork.Repository<Product>()
                .GetWithSpecAsync(spec);

            if (updatedProduct is not null)
            {
                if (hasNewImage && !string.IsNullOrEmpty(oldImage))
                    _fileService.DeleteFile(oldImage);
            }
            else
            {
                if (hasNewImage && !string.IsNullOrEmpty(product.PictureUrl))
                    _fileService.DeleteFile(product.PictureUrl);
            }

            return Ok(_mapper.Map<Product, ProductResponseDto>(updatedProduct!));
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ProductResponseDto>> Delete([FromRoute] int id)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);

            if (product is null)
                return NotFound(new ApiResponse((int)HttpStatusCode.NotFound));

            _unitOfWork.Repository<Product>().Delete(product);
            var effectedRows = await _unitOfWork.Complete();

            if (effectedRows > 0 && !string.IsNullOrEmpty(product.PictureUrl))
                _fileService.DeleteFile(product.PictureUrl);

            return NoContent();
        }
    }
}