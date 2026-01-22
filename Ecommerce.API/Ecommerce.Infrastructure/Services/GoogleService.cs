using Ecommerce.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace Ecommerce.Infrastructure.Services
{
    public class GoogleService : IGoogleService
    {
        private readonly IConfiguration _configuration;

        public GoogleService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<Payload> ValidateGoogleToken(string idToken)
        {
            var settings = new ValidationSettings()
            {
                Audience = new List<string>()
                {
                    _configuration["Authentication:Google:ClientId"]!
                }
            };

            var payload = await ValidateAsync(idToken, settings);

            return payload;
        }
    }
}