using Ecommerce.Core.Shared;
using MediatR;

namespace Ecommerce.API.Behaviours
{
    public class LoggingPipelineBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse :Result
    {
        private readonly ILogger<LoggingPipelineBehavior<TRequest, TResponse>> _logger;

        public LoggingPipelineBehavior(ILogger<LoggingPipelineBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting request {@RequestName}, {@DateTimeUtc}",
            typeof(TRequest).Name,
            DateTime.UtcNow);

            // request handler delegate
            var result = await next();

            if (result.IsFailure)
                _logger.LogError("Failure request {@RequestName}, {@Error}, {@DateTimeUtc}",
                typeof(TRequest).Name,
                result.Error,
                DateTime.UtcNow);

            _logger.LogInformation("Complete request {@RequestName}, {@DateTimeUtc}",
            typeof(TRequest).Name,
            DateTime.UtcNow);

            return result;
        }
    }
}