
namespace Ecommerce.API.Controllers
{
    [EnableRateLimiting("customer-cart")]
    public class BasketsController 
        : RedisEntityController<CustomerBasketDto, CustomerBasket>
    {
        public BasketsController(IRedisRepository<CustomerBasket> repo, IMapper mapper)
            : base(repo, mapper) {}
    }
}
