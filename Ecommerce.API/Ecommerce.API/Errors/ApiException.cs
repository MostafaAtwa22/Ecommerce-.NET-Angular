
namespace Ecommerce.API.Errors
{
    public class ApiException : ApiResponse // use for the handle exceptions middleware
    {
        public ApiException(int statusCode, string message = null!, string details = null!)
            : base(statusCode, message)
        {
            this.Details = details;
        }

        public string Details { get; set; } = string.Empty;
    }
}