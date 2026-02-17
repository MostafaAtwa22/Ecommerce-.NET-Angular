using System.Text.Json;
using Ecommerce.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace Ecommerce.API.Middlewares
{
    public class LockedUserMiddleware
    {
        private readonly RequestDelegate _next;

        public LockedUserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
        {
            var email = context.User.RetrieveEmailFromPrincipal();

            if (!string.IsNullOrWhiteSpace(email))
            {
                var user = await userManager.FindByEmailAsync(email);

                if (user != null && user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";

                    var response = new ApiResponse(StatusCodes.Status403Forbidden,
                        "Your account has been locked by an admin. Please contact support.");

                    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                    return;
                }
            }

            await _next(context);
        }
    }
}
