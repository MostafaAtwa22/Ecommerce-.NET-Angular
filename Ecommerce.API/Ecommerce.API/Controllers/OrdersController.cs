using AutoMapper;
using Ecommerce.API.Dtos;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.API.Errors;
using Ecommerce.API.Extensions;
using Ecommerce.API.Helpers.Attributes;
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

        public OrdersController(IOrderService orderService,
        IUnitOfWork unitOfWork,
        IMapper mapper)
        {
            _orderService = orderService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [Cached(100)]
        [HttpGet("getall")]
        [DisableRateLimiting]
        [AuthorizePermission(Modules.Orders, CRUD.Read)]
        public async Task<ActionResult<Pagination<AllOrdersDto>>> GetAll([FromQuery] OrdersSpecParams specParams)
        {
            var spec = new OrdersWithUserSpecification(specParams);
            var countSpec = new OrdersWithUserSpecification(specParams, true);

            var totalItems = await _unitOfWork
                .Repository<Order>()
                .CountAsync(countSpec);

            var orders = await _unitOfWork
                .Repository<Order>()
                .GetAllWithSpecAsync(spec);

            var data = _mapper.Map<IReadOnlyList<Order>, IReadOnlyList<AllOrdersDto>>(orders);
            
            return Ok(new Pagination<AllOrdersDto>(specParams.PageIndex,
                specParams.PageSize,
                totalItems,
                data));
        }

        [HttpPut("status/{id}")]
        [AuthorizePermission(Modules.Orders, CRUD.Update)]
        public async Task<ActionResult<OrderResponseDto>> UpdateOrderStatus(int id, UpdateOrderStatusDto dto)
        {
            var order = await _unitOfWork
                .Repository<Order>()
                .GetByIdAsync(id);
            
            if (order is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound));
            
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

            return Ok(_mapper.Map<Order, OrderResponseDto>(order));
        }

        [HttpGet("details/{id}")]
        [DisableRateLimiting]
        [AuthorizePermission(Modules.Orders, CRUD.Read)]
        public async Task<ActionResult<OrderResponseDto>> GetOrderDetailsById([FromRoute] int id)
        {
            var spec = new OrdersWithUserSpecification(id);
            var order = await _unitOfWork
                .Repository<Order>()
                .GetWithSpecAsync(spec);
            
            if (order is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound));
            
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