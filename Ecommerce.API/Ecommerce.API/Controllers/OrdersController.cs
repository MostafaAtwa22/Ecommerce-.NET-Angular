using AutoMapper;
using Ecommerce.API.BackgroundJobs;
using Ecommerce.API.Dtos;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.API.Errors;
using Ecommerce.API.Extensions;
using Ecommerce.API.Helpers.Attributes;
using Ecommerce.API.Helpers;
using Ecommerce.Core.Entities.orderAggregate;
using Ecommerce.Core.Interfaces;
using Ecommerce.Core.Params;
using Ecommerce.Core.Spec;
using Ecommerce.Infrastructure.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Ecommerce.API.Controllers
{
  [Authorize]
  [EnableRateLimiting("customer-orders")]
  public class OrdersController : BaseApiController
  {
    private readonly IOrderService _orderService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly OrderBackgroundService _backgroundService;

    public OrdersController(
      IOrderService orderService,
      IUnitOfWork unitOfWork,
      OrderBackgroundService backgroundService,
      IMapper mapper)
    {
      _orderService = orderService;
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
    public async Task<ActionResult<OrderResponseDto>> UpdateOrderStatus(int id, UpdateOrderStatusDto dto)
    {
      var order = await _unitOfWork.Repository<Order>()
          .GetWithSpecAsync(OrderSpecifications.BuildOrderWithItemsSpec(id));

      if (order is null)
        return this.NotFoundResponse();

      if (order.Status == OrderStatus.Complete)
        return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Completed orders cannot be modified"));

      if (order.Status == OrderStatus.Cancel)
        return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Order already cancelled"));

      if (dto.Status == OrderStatus.Cancel)
        _backgroundService.EnqueueCancelOrder(order);

      order.Status = dto.Status;

      _unitOfWork.Repository<Order>().Update(order);
      await _unitOfWork.Complete();

      return Ok(_mapper.Map<Order, OrderResponseDto>(order));
    }

    [HttpPost]
    [AuthorizePermission(Modules.Orders, CRUD.Create)]
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
    public async Task<ActionResult<OrderResponseDto>> GetOrderDetailsById([FromRoute] int id)
    {
      var spec = OrderSpecifications.BuildDetailsSpec(id);
      var order = await _unitOfWork
          .Repository<Order>()
          .GetWithSpecAsync(spec);

      if (order is null)
        return this.NotFoundResponse();

      return Ok(_mapper.Map<Order, OrderResponseDto>(order));
    }

    [HttpGet("{id}")]
    [AuthorizePermission(Modules.Orders, CRUD.Read)]
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
    public async Task<ActionResult<IReadOnlyList<OrderResponseDto>>> GetOrders()
    {
      var userEmail = HttpContext.User.RetrieveEmailFromPrincipal();
      var orders = await _orderService.GetOrdersForUserAsync(userEmail);

      return Ok(_mapper.Map<IReadOnlyList<Order>, IReadOnlyList<OrderResponseDto>>(orders));
    }
  }
}