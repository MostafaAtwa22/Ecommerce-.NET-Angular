using Ecommerce.Core.Entities;
using Ecommerce.Core.Entities.orderAggregate;
using Ecommerce.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Stripe;
using Product = Ecommerce.Core.Entities.Product;

namespace Ecommerce.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        private readonly IBasketRepository _basketRepository;

        public PaymentService(IUnitOfWork unitOfWork, 
            IConfiguration config, 
            IBasketRepository basketRepository)
        {
            _unitOfWork = unitOfWork;
            _config = config;
            _basketRepository = basketRepository;
        }

        public async Task<CustomerBasket> CreateOrUpdatePaymentIntent(string basketId)
        {
            StripeConfiguration.ApiKey = _config["StripeSettings:Secretkey"];

            var basket = await _basketRepository.GetBasketAsync(basketId);
            var shippingPrice = 0m;

            if (basket!.DeliveryMethodId.HasValue)
            {
                var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>()
                    .GetByIdAsync((int)basket.DeliveryMethodId);
                shippingPrice = deliveryMethod!.Price;
            }

            foreach (var item in basket.Items)
            {
                var productItem = await _unitOfWork.Repository<Product>()
                    .GetByIdAsync(item.Id);
                if (productItem!.Price != item.Price)
                    item.Price = productItem.Price;
            }

            // Calculate order total: items + shipping (once per order)
            var itemsTotal = basket.Items.Sum(i => i.Quantity * i.Price);
            var orderTotal = itemsTotal + shippingPrice;

            var service = new PaymentIntentService();
            PaymentIntent intent;

            if (string.IsNullOrEmpty(basket.PaymentIntentId))
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(orderTotal * 100),
                    Currency = "usd",
                    PaymentMethodTypes = new List<string> {"card"}
                };
                intent = await service.CreateAsync(options);
                basket.PaymentIntentId = intent.Id;
                basket.ClientSecret = intent.ClientSecret;
            }
            else
            {
                var options = new PaymentIntentUpdateOptions
                {
                    Amount = (long)(orderTotal * 100),
                };
                intent = await service.UpdateAsync(basket.PaymentIntentId, options);
            }

            await _basketRepository.UpdateOrCreateBasketAsync(basket);

            return basket;
        }
    }
}