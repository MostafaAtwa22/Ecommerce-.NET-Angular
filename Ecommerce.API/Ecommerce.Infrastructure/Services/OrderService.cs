using Ecommerce.Core.Entities;
using Ecommerce.Core.Entities.orderAggregate;
using Ecommerce.Core.Interfaces;
using Ecommerce.Core.Spec;

namespace Ecommerce.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisRepository<CustomerBasket> _basketRepository;
        private readonly IPaymentService _paymentService;

        public OrderService(IUnitOfWork unitOfWork,
            IRedisRepository<CustomerBasket> basketRepository,
            IPaymentService paymentService)
        {
            _unitOfWork = unitOfWork;
            _basketRepository = basketRepository;
            _paymentService = paymentService;
        }

        public async Task<Order> CreateOrderAsync(string buyerEmail, string userId, int deliverMethodId,
            string basketId, OrderAddress shippingAddress)
        {
            // get basket from the repo
            var basket = await _basketRepository.GetAsync(basketId);

            if (basket == null || !basket.Items.Any())
                throw new Exception("Basket is empty");

            // get item from product repo
            var items = new List<OrderItem>();
            foreach (var item in basket!.Items)
            {
                var productItem = await _unitOfWork.Repository<Product>().GetByIdAsync(item.Id);
                if (productItem is null)
                    throw new Exception("Product not found");

                var availableStock = productItem.Quantity - productItem.BoughtQuantity;
                if (availableStock < item.Quantity)
                    throw new Exception($"Only {availableStock} items available for {productItem.Name}");

                var itemOrdered = new ProductItemOrdered(productItem!.Id, productItem.Name, productItem.PictureUrl);

                var orderItem = new OrderItem(item.Price, item.Quantity, itemOrdered);
                items.Add(orderItem);

                productItem.BoughtQuantity += item.Quantity;
                _unitOfWork.Repository<Product>().Update(productItem);
            }

            // get delivery method
            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>()
                .GetByIdAsync(deliverMethodId);

            // calc subtotal
            decimal subTotal = items.Sum(item => item.Price * item.Quantity);

            // check to see if order exists
            var spec = new OrderByPaymentIntentIdSpecification(basket.PaymentIntentId);
            var existsOrder = await _unitOfWork.Repository<Order>().GetWithSpecAsync(spec);

            if (existsOrder is not null)
            {
                _unitOfWork.Repository<Order>().Delete(existsOrder!);
                await _paymentService.CreateOrUpdatePaymentIntent(basket.Id);
            }
            // create order
            var order = new Order(items, buyerEmail, subTotal, shippingAddress, deliveryMethod!)
            {
                ApplicationUserId = userId,
                PaymentIntenId = basket.PaymentIntentId
            };

            // save db
            await _unitOfWork.Repository<Order>().Create(order);
            int result = await _unitOfWork.Complete();

            if (result <= 0) return null!;

            // delete the basket from cache
            await _basketRepository.DeleteAsync(basketId);

            // return order
            return order;
        }

        public async Task<Order?> GetOrderByIdAsync(int id, string buyerEmail)
        {
            var orderSpec = OrderSpecifications.BuildDetailsSpec(id);

            return await _unitOfWork.Repository<Order>().GetWithSpecAsync(orderSpec)!;
        }

        public async Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string buyerEmail)
        {
            var orderSpec = new SpecificationBuilder<Order>(o => o.BuyerEmail == buyerEmail)
                .Include(o => o.DeliveryMethod)
                .Include(o => o.OrderItems)
                .OrderByDesc(o => o.OrderDate);

            return await _unitOfWork.Repository<Order>().GetAllWithSpecAsync(orderSpec)!;
        }

        public async Task CancelOrder(Order order)
        {
            var productIds = order.OrderItems
                .Select(i => i.ProductItemOrdered.ProductItemId)
                .Distinct()
                .ToList();

            var products = await _unitOfWork.Repository<Product>()
                .FindAllAsync(p => productIds.Contains(p.Id));

            foreach (var product in products)
            {
                var qty = order.OrderItems
                    .Where(i => i.ProductItemOrdered.ProductItemId == product.Id)
                    .Sum(i => i.Quantity);

                product.BoughtQuantity -= qty;
                _unitOfWork.Repository<Product>()
                    .Update(product);
            }
        }
    }
}