using Ecommerce.Core.Entities;
using Ecommerce.Core.Entities.orderAggregate;
using Ecommerce.Core.Interfaces;
using Ecommerce.Core.Spec;

namespace Ecommerce.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBasketRepository _basketRepository;

        public OrderService(IUnitOfWork unitOfWork,
            IBasketRepository basketRepository)
        {
            _unitOfWork = unitOfWork;
            _basketRepository = basketRepository;
        }

        public async Task<Order> CreateOrderAsync(string buyerEmail, int deliverMethodId,
            string basketId, OrderAddress shippingAddress)
        {
            // get basket from the repo
            var basket = await _basketRepository.GetBasketAsync(basketId);

            if (basket == null || !basket.Items.Any())
                    throw new Exception("Basket is empty");
    
            // get item from product repo
            var items = new List<OrderItem>();
            foreach (var item in basket!.Items)
            {
                var productItem = await _unitOfWork.Repository<Product>().GetByIdAsync(item.Id);
                if (productItem is null)
                    throw new Exception("Product not found");
                if (productItem.Quantity < item.Quantity)
                    throw new Exception($"Not enough stock for product: {productItem.Name}");

                var itemOrdered = new ProductItemOrdered(productItem!.Id, productItem.Name, productItem.PictureUrl);

                var orderItem = new OrderItem(item.Price, item.Quantity, itemOrdered);
                items.Add(orderItem);

                productItem.Quantity -= item.Quantity;
                _unitOfWork.Repository<Product>().Update(productItem);
            }

            // get delivery method
            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>()
                .GetByIdAsync(deliverMethodId);

            // calc subtotal
            decimal subTotal = items.Sum(item => item.Price * item.Quantity);

            // create order
            var order = new Order(items, buyerEmail, subTotal, shippingAddress, deliveryMethod!);

            // save db
            await _unitOfWork.Repository<Order>().Create(order);
            int result = await _unitOfWork.Complete();

            if (result <= 0) return null!;

            // delete the basket from cache
            await _basketRepository.DeleteBasketAsync(basketId);

            // return order
            return order;
        }

        public async Task<IReadOnlyList<DeliveryMethod>> GetDeliveryMethodsAsync()
            => await _unitOfWork.Repository<DeliveryMethod>().GetAllAsync();

        public async Task<Order?> GetOrderByIdAsync(int id, string buyerEmail)
        {
            var orderSpec = new OrderWithOrderItemsAndDeliverySpec(buyerEmail, id);

            return await _unitOfWork.Repository<Order>().GetWithSpecAsync(orderSpec)!;
        }

        public async Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string buyerEmail)
        {
            var orderSpec = new OrderWithOrderItemsAndDeliverySpec(buyerEmail);

            return await _unitOfWork.Repository<Order>().GetAllWithSpecAsync(orderSpec)!;        
        }
    }
}