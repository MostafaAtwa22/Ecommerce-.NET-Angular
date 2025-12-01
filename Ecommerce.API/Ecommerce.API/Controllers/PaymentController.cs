using Ecommerce.API.Errors;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Stripe;

namespace Ecommerce.API.Controllers
{
    public class PaymentController : BaseApiController
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<IPaymentService> _logger;
        private const string _whSecret = "whsec_f48f25cb1fe6a121547e6d585cb59179ea3615376c563110849eb7fa796b49b8";

        public PaymentController(
            IPaymentService paymentService,
            ILogger<IPaymentService> logger,
            IConfiguration config)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("{basketId}")]
        [EnableRateLimiting("customer-payment")]
        public async Task<ActionResult<CustomerBasket>> CreateOrUpdatePaymentIntent(string basketId)
        {
            var basket = await _paymentService.CreateOrUpdatePaymentIntent(basketId);

            if (basket is null)
                return BadRequest(new ApiResponse(400, "Problem with basket"));

            return Ok(basket);
        }

        [HttpPost("webhook")]
        [DisableRateLimiting]
        public async Task<ActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            Event stripeEvent;

            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _whSecret,
                    throwOnApiVersionMismatch: false
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stripe webhook error");
                return BadRequest();
            }

            PaymentIntent intent;
            Core.Entities.orderAggregate.Order order;

            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    intent = (PaymentIntent)stripeEvent.Data.Object;
                    _logger.LogInformation($"Payment succeeded: {intent.Id}");
                    order = await _paymentService.UpdateOrderPaymentSucceeded(intent.Id);
                    _logger.LogInformation($"Order updated to payment received: {order?.Id}");
                    break;

                case "payment_intent.payment_failed":
                    intent = (PaymentIntent)stripeEvent.Data.Object;
                    _logger.LogInformation($"Payment failed: {intent.Id}");
                    order = await _paymentService.UpdateOrderPaymentFailed(intent.Id);
                    _logger.LogInformation($"Order updated to payment failed: {order?.Id}");
                    break;
            }

            return new EmptyResult();
        }
    }
}
