using Ecommerce.API.BackgroundJobs;
using Ecommerce.Core.Entities.orderAggregate;
using Microsoft.AspNetCore.RateLimiting;

namespace Ecommerce.API.Controllers
{
  [Authorize]
  [EnableRateLimiting("customer-orders")]
  public class OrdersController : BaseApiController
  {
    private readonly IOrderService _orderService;
    private readonly IPaymentService _paymentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly OrderBackgroundService _backgroundService;

    public OrdersController(
      IOrderService orderService,
      IPaymentService paymentService,
      IUnitOfWork unitOfWork,
      OrderBackgroundService backgroundService,
      IMapper mapper)
    {
      _orderService = orderService;
      _paymentService = paymentService;
      _unitOfWork = unitOfWork;
      _backgroundService = backgroundService;
      _mapper = mapper;
    }

    [Cached(100)]
    [HttpGet("getall")]
    [DisableRateLimiting]
    [AuthorizePermission(Modules.Orders, CRUD.Read)]
    public async Task<ActionResult<Pagination<AllOrdersDto>>> GetAll([FromQuery] OrdersSpecParams specParams)
    {
      var dataSpec = OrderSpecifications.BuildListingSpec(specParams);
      var countSpec = OrderSpecifications.BuildListingCountSpec(specParams);

      return await this.ToPagedResultAsync<Order, AllOrdersDto>(
          _unitOfWork,
          dataSpec,
          countSpec,
          specParams.PageIndex,
          specParams.PageSize,
          _mapper);
    }

    [HttpPut("status/{id}")]
    [AuthorizePermission(Modules.Orders, CRUD.Update)]
    [InvalidateCache("/api/orders")]
    public async Task<ActionResult<OrderResponseDto>> UpdateOrderStatus(int id, UpdateOrderStatusDto dto)
    {
      var (order, errorResponse) = await GetOrderForUpdateOrError(id);
      if (order is null) return errorResponse!;

      if (!CanModifyStatus(order.Status, out var error))
        return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, error));

      if (dto.Status == OrderStatus.Cancel)
        _backgroundService.EnqueueCancelOrder(order);

      order.Status = dto.Status;

      await _unitOfWork.Complete();

      return Ok(_mapper.Map<Order, OrderResponseDto>(order));
    }

    [HttpPost("{id}/cancel")]
    [AuthorizePermission(Modules.Orders, CRUD.Create)]
    [InvalidateCache("/api/orders")]
    public async Task<ActionResult<OrderResponseDto>> CancelOrderByUser([FromRoute] int id)
    {
      var userEmail = HttpContext.User.RetrieveEmailFromPrincipal();
      if (string.IsNullOrWhiteSpace(userEmail))
        return Unauthorized(new ApiResponse(401, "User not authenticated"));

      var (order, errorResponse) = await GetOrderForUpdateOrError(id);
      if (order is null) return errorResponse!;

      if (!CanCancel(order.Status, out var error))
        return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, error));

      var wasPaymentReceived = order.Status == OrderStatus.PaymentReceived;

      order.Status = OrderStatus.Cancel;

      _backgroundService.EnqueueCancelOrder(order);

      if (wasPaymentReceived && !string.IsNullOrWhiteSpace(order.PaymentIntenId))
        await _paymentService.RefundPaymentIntentAsync(order.PaymentIntenId, null, "requested_by_customer");

      await _unitOfWork.Complete();
      return Ok(_mapper.Map<Order, OrderResponseDto>(order));
    }

    [HttpPost("{id}/return")]
    [InvalidateCache("/api/orders")]
    public async Task<ActionResult<OrderResponseDto>> RequestReturnByUser([FromRoute] int id)
    {
      var userEmail = HttpContext.User.RetrieveEmailFromPrincipal();
      if (string.IsNullOrWhiteSpace(userEmail))
        return Unauthorized(new ApiResponse(401, "User not authenticated"));

      var (order, errorResponse) = await GetOrderForUpdateOrError(id);
      if (order is null) return errorResponse!;

      if (order.Status != OrderStatus.Complete)
        return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Only completed orders can be returned"));

      order.Status = OrderStatus.ReturnRequested;
      await _unitOfWork.Complete();

      return Ok(_mapper.Map<Order, OrderResponseDto>(order));
    }

    [HttpPost("{id}/return/approve")]
    [AuthorizePermission(Modules.Orders, CRUD.Update)]
    [InvalidateCache("/api/orders")]
    public async Task<ActionResult<OrderResponseDto>> ApproveReturn([FromRoute] int id)
    {
      var (order, errorResponse) = await GetOrderForUpdateOrError(id);
      if (order is null) return errorResponse!;

      if (order.Status != OrderStatus.ReturnRequested)
        return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Order is not in return requested state"));

      if (string.IsNullOrWhiteSpace(order.PaymentIntenId))
        return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Order has no payment intent id to refund"));

      var amountInCents = (long)(order.SubTotal * 100);
      await _paymentService.RefundPaymentIntentAsync(order.PaymentIntenId, amountInCents, "requested_by_customer");

      order.Status = OrderStatus.Refunded;
      await _unitOfWork.Complete();

      return Ok(_mapper.Map<Order, OrderResponseDto>(order));
    }

    [HttpPost]
    [AuthorizePermission(Modules.Orders, CRUD.Create)]
    [InvalidateCache("/api/orders")]
    public async Task<ActionResult<OrderResponseDto>> CreateOrder(OrderDto dto)
    {
      var userEmail = HttpContext.User.RetrieveEmailFromPrincipal();
      var userId = HttpContext.User.RetrieveUserIdFromPrincipal();

      if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(userId))
        return Unauthorized(new ApiResponse(401, "User not authenticated"));

      var addressDto = _mapper.Map<OrderAddressDto, OrderAddress>(dto.ShipToAddress);

      var order = await _orderService.CreateOrderAsync(userEmail, userId, dto.DeliveryMethodId, dto.BasketId, addressDto);

      if (order is null)
        return BadRequest(new ApiResponse(400, "Problem creating order"));

      _backgroundService.EnqueueSendEmail(order);

      return Ok(_mapper.Map<Order, OrderResponseDto>(order));
    }

    [HttpGet("details/{id}")]
    [DisableRateLimiting]
    [AuthorizePermission(Modules.Orders, CRUD.Read)]
    [InvalidateCache("/api/orders")]
    public async Task<ActionResult<OrderResponseDto>> GetOrderDetailsById([FromRoute] int id)
    {
      var (order, errorResponse) = await GetOrderForDetailsOrError(id);
      if (order is null) return errorResponse!;

      return Ok(_mapper.Map<Order, OrderResponseDto>(order));
    }

    [HttpGet("{id}")]
    [AuthorizePermission(Modules.Orders, CRUD.Read)]
    [Cached(100)]
    public async Task<ActionResult<OrderResponseDto>> GetOrderById([FromRoute] int id)
    {
      var userEmail = HttpContext.User.RetrieveEmailFromPrincipal();
      var order = await _orderService.GetOrderByIdAsync(id, userEmail);

      if (order is null)
        return NotFound(new ApiResponse(StatusCodes.Status404NotFound));

      return Ok(_mapper.Map<Order, OrderResponseDto>(order));
    }

    [HttpGet]
    [AuthorizePermission(Modules.Orders, CRUD.Read)]
    [Cached(200)]
    public async Task<ActionResult<IReadOnlyList<OrderResponseDto>>> GetOrders()
    {
      var userEmail = HttpContext.User.RetrieveEmailFromPrincipal();
      var orders = await _orderService.GetOrdersForUserAsync(userEmail);

      return Ok(_mapper.Map<IReadOnlyList<Order>, IReadOnlyList<OrderResponseDto>>(orders));
    }

    private static bool CanModifyStatus(OrderStatus status, out string error)
    {
      error = status switch
      {
        OrderStatus.Complete => "Completed orders cannot be modified",
        OrderStatus.Refunded => "Refunded orders cannot be modified",
        OrderStatus.Cancel => "Order already cancelled",
        OrderStatus.ReturnRequested => "Return requested orders cannot be modified from here",
        _ => null!
      };
      return error is null;
    }

    private static bool CanCancel(OrderStatus status, out string error)
    {
      error = status switch
      {
        OrderStatus.Cancel => "Order already cancelled",
        OrderStatus.Shipped or OrderStatus.Complete => "Order cannot be cancelled after shipping",
        OrderStatus.Refunded => "Refunded orders cannot be cancelled",
        OrderStatus.ReturnRequested => "Return requested orders cannot be cancelled",
        _ => null!
      };
      return error is null;
    }

    private async Task<(Order? order, ActionResult<OrderResponseDto>? error)> GetOrderOrError(int id)
    {
      var spec = OrderSpecifications.BuildDetailsSpec(id);
      var order = await _unitOfWork.Repository<Order>().GetWithSpecAsync(spec);

      ActionResult<OrderResponseDto>? errorResponse = null;
      if (order is null)
        errorResponse = NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Order not found"));

      return (order, errorResponse);
    }

    private async Task<(Order? order, ActionResult<OrderResponseDto>? error)> GetOrderForDetailsOrError(int id)
    {
      var spec = OrderSpecifications.BuildDetailsSpec(id);
      var order = await _unitOfWork.Repository<Order>().GetWithSpecAsync(spec);

      ActionResult<OrderResponseDto>? errorResponse = null;
      if (order is null)
        errorResponse = NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Order not found"));

      return (order, errorResponse);
    }

    private async Task<(Order? order, ActionResult<OrderResponseDto>? error)> GetOrderForUpdateOrError(int id)
    {
      var spec = new SpecificationBuilder<Order>(o => o.Id == id)
        .Include(o => o.OrderItems)
        .WithTracking();

      var order = await _unitOfWork.Repository<Order>().GetWithSpecAsync(spec);

      ActionResult<OrderResponseDto>? errorResponse = null;
      if (order is null)
        errorResponse = NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Order not found"));

      return (order, errorResponse);
    }
  }
}