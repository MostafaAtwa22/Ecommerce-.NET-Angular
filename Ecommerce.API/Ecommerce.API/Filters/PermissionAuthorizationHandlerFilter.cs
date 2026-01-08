using Ecommerce.Infrastructure.Constants;
using Microsoft.AspNetCore.Authorization;

namespace Ecommerce.API.Filters
{
    public class PermissionAuthorizationHandlerFilter : AuthorizationHandler<PermissionRequirementFilter>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirementFilter requirement)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
                return Task.CompletedTask;

            var hasPermission = context.User.Claims.Any(c =>
                c.Type == Permissions.ClaimType &&
                c.Value == requirement.Permission);

            if (hasPermission)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}