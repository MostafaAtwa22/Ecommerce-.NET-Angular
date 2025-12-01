using AutoMapper;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces;
using Microsoft.AspNetCore.RateLimiting;

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
