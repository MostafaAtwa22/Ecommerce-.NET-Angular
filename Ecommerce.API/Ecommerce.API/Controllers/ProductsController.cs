using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ILogger<ProductsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string GetAll()
        {
            return "this will be a list of products";
        }

        [HttpGet("{id}")]
        public string GetById(int id)
        {
            return "this will be a single product";
        }
    }
}