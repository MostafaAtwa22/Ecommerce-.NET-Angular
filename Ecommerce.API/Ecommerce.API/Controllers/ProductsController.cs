using System.Net;
using AutoMapper;
using Ecommerce.API.Dtos;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.API.Errors;
using Ecommerce.API.Helpers.Attributes;
using Ecommerce.API.Helpers;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces;
using Ecommerce.Core.Params;
using Ecommerce.Core.Spec;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Ecommerce.Infrastructure.Constants;

namespace Ecommerce.API.Controllers
{
    [EnableRateLimiting("customer-browsing")]
    public class ProductsController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly IMapper _mapper;

        public ProductsController(
            IUnitOfWork unitOfWork,
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
            var dataSpec = ProductSpecifications.BuildListingSpec(specParams);
            var countSpec = ProductSpecifications.BuildListingCountSpec(specParams);

            return await this.ToPagedResultAsync<Product, ProductResponseDto>(
                _unitOfWork,
                dataSpec,
                countSpec,
                specParams.PageIndex,
                specParams.PageSize,
                _mapper);
        }

        [Cached(600)]
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductResponseDto>> GetById([FromRoute] int id)
        {
            var spec = ProductSpecifications.BuildDetailsSpec(id);
            var product = await _unitOfWork.Repository<Product>()
                .GetWithSpecAsync(spec);

            if (product is null)
                return this.NotFoundResponse();

            return Ok(_mapper.Map<Product, ProductResponseDto>(product));
        }

        [HttpPost]
        [AuthorizePermission(Modules.Products, CRUD.Create)]
        [DisableRateLimiting]
        [InvalidateCache("/api/products")]
        public async Task<ActionResult<ProductResponseDto>> Create([FromForm] ProductCreationDto creationDto)
        {
            var product = _mapper.Map<ProductCreationDto, Product>(creationDto);

            product.PictureUrl = await _fileService.SaveFileAsync(creationDto.ImageFile, "products");

            await _unitOfWork.Repository<Product>().Create(product);

            await _unitOfWork.Complete();

            var spec = ProductSpecifications.BuildDetailsSpec(product.Id);
            var createdProduct = await _unitOfWork.Repository<Product>()
                .GetWithSpecAsync(spec);

            return Ok(_mapper.Map<Product, ProductResponseDto>(createdProduct!));
        }

        [HttpPut]
        [AuthorizePermission(Modules.Products, CRUD.Update)]
        [DisableRateLimiting]
        [InvalidateCache("/api/products")]
        public async Task<ActionResult<ProductResponseDto>> Update([FromForm] ProductUpdateDto updateDto)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(updateDto.ProductId);

            if (product is null)
                return this.NotFoundResponse();

            var hasNewImage = updateDto.ImageFile is not null;
            var oldImage = product.PictureUrl;

            _mapper.Map(updateDto, product);

            if (hasNewImage)
                product.PictureUrl = await _fileService.SaveFileAsync(updateDto.ImageFile!, "products");

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                _unitOfWork.Repository<Product>().Update(product);
                await _unitOfWork.Complete();

                await transaction.CommitAsync();

                if (hasNewImage && !string.IsNullOrEmpty(oldImage))
                    _fileService.DeleteFile(oldImage);
            }
            catch
            {
                if (hasNewImage && !string.IsNullOrEmpty(product.PictureUrl))
                    _fileService.DeleteFile(product.PictureUrl);

                await transaction.RollbackAsync();
                throw;
            }

            var spec = ProductSpecifications.BuildDetailsSpec(product.Id);
            var updatedProduct = await _unitOfWork.Repository<Product>()
                .GetWithSpecAsync(spec);

            return Ok(_mapper.Map<Product, ProductResponseDto>(updatedProduct!));
        }

        [HttpDelete("{id:int}")]
        [AuthorizePermission(Modules.Products, CRUD.Delete)]
        [DisableRateLimiting]
        [InvalidateCache("/api/products")]
        public async Task<ActionResult<ProductResponseDto>> Delete([FromRoute] int id)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);

            if (product is null)
                return this.NotFoundResponse();

            _unitOfWork.Repository<Product>().Delete(product);
            var effectedRows = await _unitOfWork.Complete();

            if (effectedRows > 0 && !string.IsNullOrEmpty(product.PictureUrl))
                _fileService.DeleteFile(product.PictureUrl);

            return NoContent();
        }
    }
}