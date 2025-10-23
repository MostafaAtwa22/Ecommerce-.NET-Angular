
namespace Ecommerce.API.Errors
{
    public class ApiResponse
    {
        public ApiResponse()
        {
        }
        public ApiResponse(int statusCode, string message = null!)
        {
            this.StatusCode = statusCode;
            this.Message = message ?? GetDefaultMessageForStatusCode(statusCode);
        }

        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;

        private string GetDefaultMessageForStatusCode(int statusCode)
            => statusCode switch
            {
                400 => "A bad request, you have made",
                401 => "Authorized, you are not",
                403 => "Forbidden from doing this, you are",
                404 => "Resource found, it was not",
                500 => "Internal server error occurred",
                _ => null!
            };
    }
}