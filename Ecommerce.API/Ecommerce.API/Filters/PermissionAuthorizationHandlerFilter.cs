using Ecommerce.Infrastructure.Constants;
using Microsoft.AspNetCore.Authorization;
using Ecommerce.Core.Interfaces;
using Ecommerce.API.Extensions;

namespace Ecommerce.API.Filters
{
    public class PermissionAuthorizationHandlerFilter : AuthorizationHandler<PermissionRequirementFilter>
    {
        private readonly IPermissionService _permissionService;

        public PermissionAuthorizationHandlerFilter(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirementFilter requirement)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
                return;

            var userId = context.User.RetrieveUserIdFromPrincipal();
            
            if (string.IsNullOrEmpty(userId))
                return;

            var hasPermission = await _permissionService.HasPermissionAsync(userId, requirement.Permission);

            if (hasPermission)
                context.Succeed(requirement);
        }
    }
}