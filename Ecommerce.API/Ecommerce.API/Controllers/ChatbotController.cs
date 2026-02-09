using Ecommerce.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [AllowAnonymous]
    public class ChatbotController : BaseApiController
    {
        private readonly IChatbotService _chatbotService;

        public ChatbotController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> AskBot([FromBody] string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return BadRequest(new { Message = "Message cannot be empty" });
            }

            var response = await _chatbotService.GetResponseAsync(message);
            
            return Ok(new 
            { 
                Response = response,
                Timestamp = DateTime.UtcNow 
            });
        }
    }
}
