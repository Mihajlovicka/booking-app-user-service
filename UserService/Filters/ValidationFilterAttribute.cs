using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using UserService.Middlewares;

namespace UserService.Filters;

public class ValidationFilterAttribute : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errorMessages = context
                .ModelState.Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            context.Result = new BadRequestObjectResult(
                new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = string.Join(", \n", errorMessages),
                }
            );
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
