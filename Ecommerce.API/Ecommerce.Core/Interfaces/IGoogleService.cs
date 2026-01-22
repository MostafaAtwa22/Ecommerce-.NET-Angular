using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace Ecommerce.Core.Interfaces
{
    public interface IGoogleService
    {
        Task<Payload> ValidateGoogleToken(string idToken);
    }
}