using Ecommerce.Infrastructure.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Ecommerce.API.Filters
{
    public class PermissionPolicyProviderFilter : IAuthorizationPolicyProvider
    {
        public DefaultAuthorizationPolicyProvider DefaultProvider { get; }

        public PermissionPolicyProviderFilter(IOptions<AuthorizationOptions> options)
        {
            DefaultProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
            => DefaultProvider.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
            => DefaultProvider.GetFallbackPolicyAsync();

        public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(Permissions.ClaimType, StringComparison.OrdinalIgnoreCase))
            {
                var policy = new AuthorizationPolicyBuilder();
                policy.AddRequirements(new PermissionRequirementFilter(policyName));
                return await Task.FromResult(policy.Build());
            }
            return await DefaultProvider.GetPolicyAsync(policyName);
        }
    }
}