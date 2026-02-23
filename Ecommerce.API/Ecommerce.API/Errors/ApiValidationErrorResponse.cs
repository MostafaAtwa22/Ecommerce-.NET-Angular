
namespace Ecommerce.API.Errors
{
    public class ApiValidationErrorResponse : ApiResponse
    {
        public ApiValidationErrorResponse()
            : base((int)HttpStatusCode.BadRequest)
        {
            Errors = [];
        }
        public IEnumerable<string> Errors { get; set; } 
    }
}
