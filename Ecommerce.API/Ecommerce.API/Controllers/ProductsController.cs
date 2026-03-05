namespace Ecommerce.API.Controllers
{
    [EnableRateLimiting("customer-browsing")]
    public class ProductsController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public ProductsController(
            IUnitOfWork unitOfWork,
            IProductService productService,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _productService = productService;
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

        [Cached(60)]
        [HttpGet("suggestions")]
        public async Task<ActionResult<IReadOnlyList<ProductSuggestionDto>>> GetSuggestions(
            [FromQuery] string? term,
            [FromQuery] int? brandId,
            [FromQuery] int? typeId,
            [FromQuery] int limit = 8)
        {
            var normalizedTerm = term?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedTerm))
            {
                return Ok(Array.Empty<ProductSuggestionDto>());
            }

            var normalizedLimit = Math.Clamp(limit, 1, 10);
            var spec = ProductSpecifications.BuildSuggestionsSpec(normalizedTerm, brandId, typeId, normalizedLimit);
            var products = await _unitOfWork.Repository<Product>().GetAllWithSpecAsync(spec);

            return Ok(_mapper.Map<IReadOnlyList<ProductSuggestionDto>>(products));
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
            var product = await _productService.CreateProductAsync(creationDto);

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
            var product = await _productService.UpdateProductAsync(updateDto);

            if (product is null)
                return this.NotFoundResponse();

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
            var success = await _productService.DeleteProductAsync(id);

            if (!success)
                return this.NotFoundResponse();

            return NoContent();
        }
    }
}
