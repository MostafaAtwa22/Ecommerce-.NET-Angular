using Stripe;
using Product = Ecommerce.Core.Entities.Product;
using CoreOrder = Ecommerce.Core.Entities.orderAggregate.Order;

namespace Ecommerce.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        private readonly IRedisRepository<CustomerBasket> _basketRepository;

        public PaymentService(
            IUnitOfWork unitOfWork,
            IConfiguration config,
            IRedisRepository<CustomerBasket> basketRepository)
        {
            _unitOfWork = unitOfWork;
            _config = config;
            _basketRepository = basketRepository;
        }

        public async Task<CustomerBasket> CreateOrUpdatePaymentIntent(string basketId)
        {
            StripeConfiguration.ApiKey = _config["StripeSettings:Secretkey"];

            var basket = await _basketRepository.GetAsync(basketId);

            if (basket is null)
                return null!;

            decimal shippingPrice = 0m;

            if (basket.DeliveryMethodId.HasValue)
            {
                var deliveryMethod =
                    await _unitOfWork.Repository<DeliveryMethod>()
                        .GetByIdAsync(basket.DeliveryMethodId.Value);

                shippingPrice = deliveryMethod!.Price;
            }

            foreach (var item in basket.Items)
            {
                var product = await _unitOfWork.Repository<Product>().GetByIdAsync(item.Id);

                if (product is null)
                    continue;

                if (product.Price != item.Price)
                    item.Price = product.Price;
            }

            var total = basket.Items.Sum(x => x.Price * x.Quantity) + shippingPrice;

            var service = new PaymentIntentService();
            PaymentIntent intent;

            if (string.IsNullOrEmpty(basket.PaymentIntentId))
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(total * 100),
                    Currency = "usd",
                    PaymentMethodTypes = new List<string> { "card" }
                };

                intent = await service.CreateAsync(options);
                basket.PaymentIntentId = intent.Id;
                basket.ClientSecret = intent.ClientSecret;
            }
            else
            {
                var options = new PaymentIntentUpdateOptions
                {
                    Amount = (long)(total * 100)
                };

                intent = await service.UpdateAsync(basket.PaymentIntentId, options);
            }

            await _basketRepository.UpdateOrCreateAsync(basket.Id, basket);

            return basket;
        }

        public async Task<CoreOrder> UpdateOrderPaymentFailed(string paymentIntentId)
        {
            var spec = new OrderByPaymentIntentIdSpecification(paymentIntentId);
            var order = await _unitOfWork.Repository<CoreOrder>().GetWithSpecAsync(spec);

            if (order is null)
                return null!;

            order.Status = OrderStatus.PaymentFailed;
            _unitOfWork.Repository<CoreOrder>().Update(order);
            await _unitOfWork.Complete();

            return order;
        }

        public async Task<CoreOrder> UpdateOrderPaymentSucceeded(string paymentIntentId)
        {
            var spec = new OrderByPaymentIntentIdSpecification(paymentIntentId);
            var order = await _unitOfWork.Repository<CoreOrder>().GetWithSpecAsync(spec);

            if (order is null)
                return null!;

            order.Status = OrderStatus.PaymentReceived;
            _unitOfWork.Repository<CoreOrder>().Update(order);
            await _unitOfWork.Complete();

            return order;
        }

        public async Task RefundPaymentIntentAsync(string paymentIntentId, long? amountInCents, string reason)
        {
            StripeConfiguration.ApiKey = _config["StripeSettings:Secretkey"];

            if (string.IsNullOrWhiteSpace(paymentIntentId))
                throw new ArgumentException("Payment intent id is required", nameof(paymentIntentId));

            var refundOptions = new RefundCreateOptions
            {
                PaymentIntent = paymentIntentId,
                Reason = reason,
            };

            if (amountInCents.HasValue)
                refundOptions.Amount = amountInCents.Value;

            var refundService = new RefundService();
            await refundService.CreateAsync(refundOptions);
        }
    }
}
