using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    public class BasketsController : BaseApiController
    {
        private readonly IBasketRepository _basketRepository;

        public BasketsController(IBasketRepository basketRepository)
        {
            _basketRepository = basketRepository;
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerBasket>> GetById(string id)
        {
            var basket = await _basketRepository.GetBasketAsync(id);
            return Ok(basket ?? new CustomerBasket(id));
        }

        [HttpPost]
        public async Task<ActionResult<CustomerBasket>> UpdateOrCreatec([FromBody]CustomerBasket basket)
        {
            var newBasket = await _basketRepository.UpdateOrCreateBasketAsync(basket);

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
