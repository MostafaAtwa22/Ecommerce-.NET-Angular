using AutoMapper;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    public class BasketsController 
        : RedisEntityController<CustomerBasketDto, CustomerBasket>
    {
        public BasketsController(IRedisRepository<CustomerBasket> repo, IMapper mapper)
            : base(repo, mapper) {}
    }
}
