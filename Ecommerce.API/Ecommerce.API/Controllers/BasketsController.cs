using AutoMapper;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    public class BasketsController : BaseApiController
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IMapper _mapper;

        public BasketsController(IBasketRepository basketRepository,
            IMapper mapper)
        {
            _basketRepository = basketRepository;
            _mapper = mapper;
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerBasket>> GetById(string id)
        {
            var basket = await _basketRepository.GetBasketAsync(id);
            return Ok(basket ?? new CustomerBasket(id));
        }

        [HttpPost]
        public async Task<ActionResult<CustomerBasketDto>> UpdateOrCreatec([FromBody]CustomerBasketDto basket)
        {
            var customerBasket = _mapper.Map<CustomerBasketDto, CustomerBasket>(basket);

            var newBasket = await _basketRepository.UpdateOrCreateBasketAsync(customerBasket);

            return Ok(newBasket);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete([FromRoute]string id)
        {
            var result = await _basketRepository.DeleteBasketAsync(id);
            if (!result)
                return NotFound($"Basket with ID '{id}' not found.");

            return NoContent();
        }
    }
}
