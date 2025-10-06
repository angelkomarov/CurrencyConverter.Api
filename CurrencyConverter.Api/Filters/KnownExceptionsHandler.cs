using CurrencyConverter.Api.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using System.ComponentModel.DataAnnotations;

namespace CurrencyConverter.Api.Filters
{
    //!!AK3.1 implement exception handles for known exceptions
    //https://learn.microsoft.com/en-us/aspnet/core/web-api/handle-errors?view=aspnetcore-8.0
    public class KnownExceptionsHandler(ILogger<KnownExceptionsHandler> logger)
        : IExceptionHandler
    {
        private static readonly IDictionary<Type, Func<HttpContext, Exception, IResult>> ExceptionHandlers = new Dictionary<Type, Func<HttpContext, Exception, IResult>>
    {
        { typeof(ValidationException), HandleValidationException },
         { typeof(ExternalApiUnavailableException), HandleExternalApiUnavailableException },
        { typeof(ExternalApiException), HandleExternalApiException }
    };

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            Type type = exception.GetType();

            if (ExceptionHandlers.TryGetValue(type, out var handler))
            {
                IResult result = handler.Invoke(httpContext, exception);
                await result.ExecuteAsync(httpContext);
                return true;
            }
            else
                logger.LogError(exception, "*** Unhandled exception:", exception.StackTrace);

            return false;
        }

        private static IResult HandleValidationException(HttpContext context, Exception exception)
        {
            var validationException = exception as ValidationException ?? throw new InvalidOperationException("Exception is not of type ValidationException");

            return Results.Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                detail: validationException.Message);

        }
        private static IResult HandleExternalApiUnavailableException(HttpContext context, Exception exception)
        {
            var externalApiUnavailableException = exception as ExternalApiUnavailableException ?? throw new InvalidOperationException("Exception is not of type ExternalApiUnavailableException");

            return Results.Problem(statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "External API error",
                type: "https://tools.ietf.org/html/rfc7231#section-6.6.4",
                detail: externalApiUnavailableException.Message);
        }

        private static IResult HandleExternalApiException(HttpContext context, Exception exception)
        {
            var externalApiException = exception as ExternalApiException ?? throw new InvalidOperationException("Exception is not of type ExternalApiException");

            return Results.Problem(statusCode: StatusCodes.Status502BadGateway,
                title: "External API error",
                type: "https://tools.ietf.org/html/rfc7231#section-6.6.3",
                detail: externalApiException.Message);
        }

        private static IResult HandleNotFoundException(HttpContext context, Exception exception) =>
            Results.Problem(statusCode: StatusCodes.Status404NotFound,
                title: "The specified resource was not found",
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                detail: exception.Message);

    }
}
