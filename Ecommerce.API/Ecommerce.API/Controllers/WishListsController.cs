namespace Ecommerce.API.Controllers
{
    [EnableRateLimiting("customer-wishlist")]
    public class WishListsController 
        : RedisEntityController<CustomerWishListDto, CustomerWishList>
    {
        public WishListsController(IRedisRepository<CustomerWishList> repo, IMapper mapper)
            : base(repo, mapper) {}
    }
}
