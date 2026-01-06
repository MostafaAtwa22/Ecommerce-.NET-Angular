using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Infrastructure.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Ecommerce.API.Filters
{
    public class PermissionAuthorizationHandlerFilter : AuthorizationHandler<PermissionRequirementFilter>
    {
        private readonly string _issuer;

        public PermissionAuthorizationHandlerFilter(IConfiguration config)
        {
            _issuer = config["Token:Issuer"]!;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirementFilter requirement)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
                return Task.CompletedTask;

            var hasPermission = context.User.Claims.Any(c =>
                c.Type == Permissions.ClaimType &&
                c.Value == requirement.Permission &&
                c.Issuer == _issuer);

            if (hasPermission)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}