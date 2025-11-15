using AutoMapper;
using Ecommerce.API.Dtos;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.API.Errors;
using Ecommerce.API.Extensions;
using Ecommerce.Core.Entities.orderAggregate;
using Ecommerce.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [Authorize]
    public class OrdersController : BaseApiController
    {
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;

        public OrdersController(IOrderService orderService,
        IMapper mapper)
        {
            _orderService = orderService;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<OrderResponseDto>> CreateOrder(OrderDto dto)
        {
            var userEmail = HttpContext.User.RetrieveEmailFromPrincipal();

            var addressDto = _mapper.Map<OrderAddressDto, OrderAddress>(dto.ShipToAddress);

            var order = await _orderService.CreateOrderAsync(userEmail, dto.DeliveryMethodId, dto.BasketId, addressDto);

            if (order is null)
                return BadRequest(new ApiResponse(400, "Problem creating order"));
            
            return Ok(_mapper.Map<Order, OrderResponseDto>(order));
        }

        [HttpGet("deliveryMethods")]
        public async Task<ActionResult<IReadOnlyList<DeliveryMethod>>> GetAllDeliveryMethods()
            => Ok(await _orderService.GetDeliveryMethodsAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponseDto>> GetOrderById([FromRoute] int id)
        {
            var userEmail = HttpContext.User.RetrieveEmailFromPrincipal();
            var order = await _orderService.GetOrderByIdAsync(id, userEmail);
            
            if (order is null)
                return NotFound(new ApiResponse(404, "Order not found"));
            
            return Ok(_mapper.Map<Order, OrderResponseDto>(order));
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<OrderResponseDto>>> GetOrders()
        {
            var userEmail = HttpContext.User.RetrieveEmailFromPrincipal();
            var orders = await _orderService.GetOrdersForUserAsync(userEmail);

            return Ok(_mapper.Map<IReadOnlyList<Order>, IReadOnlyList<OrderResponseDto>>(orders));
        }
    }
}