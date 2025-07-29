using Azure;
using iText.Kernel.Geom;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Survey.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context) //'ExceptionContext' contains all the information about the exception and the context in which it occurred(like which controller/action was running).
        {
            _logger.LogError(context.Exception, "An unhandled exception occurred in an MVC action.");

            var problemDetails = new ProblemDetails
            {
                Status = 500,
                Title = "An unexpected error occurred.",
                Detail = "An internal server error occurred."
            };
            // We set the 'Result' of the action. This tells MVC to stop processing the request and immediately send this object back as the response.
            // 'ObjectResult' is a type of 'IActionResult' that sends a given object as the response body.
            context.Result = new ObjectResult(problemDetails)
            {
                StatusCode = 500
            };

            context.ExceptionHandled = true;
        }
    }
}
