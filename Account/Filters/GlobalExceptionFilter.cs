using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Account.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Account.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var statusCode = context.Exception switch
            {
                NotFoundException => StatusCodes.Status404NotFound,
                ApiException or ValidationException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError
            };

            context.Result = new ObjectResult(new
            {
                error = context.Exception.Message
            })
            {
                StatusCode = statusCode
            };
        }
    }

}
