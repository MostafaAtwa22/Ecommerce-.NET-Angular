using AutoMapper;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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
