
namespace Ecommerce.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisRepository<CustomerBasket> _basketRepository;
        private readonly IPaymentService _paymentService;
        private readonly ICouponService _couponService;

        public OrderService(IUnitOfWork unitOfWork,
            IRedisRepository<CustomerBasket> basketRepository,
            IPaymentService paymentService,
            ICouponService couponService)
        {
            _unitOfWork = unitOfWork;
            _basketRepository = basketRepository;
            _paymentService = paymentService;
            _couponService = couponService;
        }

        public async Task<Order> CreateOrderAsync(string buyerEmail, string userId, int deliverMethodId,
            string basketId, OrderAddress shippingAddress, string? couponCode = null)
        {
            var basket = await _basketRepository.GetAsync(basketId);
            if (basket == null || !basket.Items.Any())
                throw new BadRequestException("Basket is empty");

            var items = await BuildOrderItemsAsync(basket);

            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(deliverMethodId);

            decimal subTotal = items.Sum(item => item.Price * item.Quantity);

            var (coupon, discount) = await ResolveCouponAsync(couponCode, basket.CouponCode);

            await HandleExistingOrderAsync(basket.PaymentIntentId, basketId);

            var order = new Order(items, buyerEmail, subTotal, shippingAddress, deliveryMethod!)
            {
                ApplicationUserId = userId,
                PaymentIntenId = basket.PaymentIntentId,
                Coupon = coupon,
                Discount = discount
            };

            await _unitOfWork.Repository<Order>().Create(order);
            int result = await _unitOfWork.Complete();

            if (result <= 0) return null!;

            await _basketRepository.DeleteAsync(basketId);

            return order;
        }

        private async Task<List<OrderItem>> BuildOrderItemsAsync(CustomerBasket basket)
        {
            var productIds = basket.Items.Select(i => i.Id).ToList();
            var products = await _unitOfWork.Repository<Product>().FindAllAsync(p => productIds.Contains(p.Id));

            var items = new List<OrderItem>();
            foreach (var item in basket.Items)
            {
                var productItem = products.FirstOrDefault(p => p.Id == item.Id);
                if (productItem is null)
                    throw new NotFoundException($"Product with ID {item.Id} not found");

                var availableStock = productItem.Quantity - productItem.BoughtQuantity;
                if (availableStock < item.Quantity)
                    throw new BadRequestException($"Only {availableStock} items available for {productItem.Name}");

                var itemOrdered = new ProductItemOrdered(productItem.Id, productItem.Name, productItem.PictureUrl, productItem.Discount.Percentage);
                var currentPrice = productItem.IsDiscounted ? productItem.DiscountedPrice : productItem.Price;
                var orderItem = new OrderItem(currentPrice, item.Quantity, itemOrdered);
                items.Add(orderItem);

                productItem.BoughtQuantity += item.Quantity;
                _unitOfWork.Repository<Product>().Update(productItem);
            }
            return items;
        }

        private async Task<(Coupon? coupon, decimal discount)> ResolveCouponAsync(string? requestedCode, string? basketCode)
        {
            var codeToUse = !string.IsNullOrEmpty(requestedCode) ? requestedCode : basketCode;

            if (string.IsNullOrEmpty(codeToUse)) return (null, 0);

            var coupon = await _couponService.GetValidCouponAsync(codeToUse);
            return coupon != null ? (coupon, coupon.DiscountAmount) : (null, 0);
        }

        private async Task HandleExistingOrderAsync(string paymentIntentId, string basketId)
        {
            var spec = new OrderByPaymentIntentIdSpecification(paymentIntentId);
            var existsOrder = await _unitOfWork.Repository<Order>().GetWithSpecAsync(spec);

            if (existsOrder is not null)
            {
                _unitOfWork.Repository<Order>().Delete(existsOrder!);
                await _paymentService.CreateOrUpdatePaymentIntent(basketId);
            }
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
