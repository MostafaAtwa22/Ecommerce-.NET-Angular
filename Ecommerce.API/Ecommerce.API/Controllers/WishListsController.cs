using AutoMapper;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces;

namespace Ecommerce.API.Controllers
{
    public class WishListsController 
        : RedisEntityController<CustomerWishListDto, CustomerWishList>
    {
        public WishListsController(IRedisRepository<CustomerWishList> repo, IMapper mapper)
            : base(repo, mapper) {}
    }
}
