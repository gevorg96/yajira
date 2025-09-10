using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace YetAnotherJira.Filters;

public class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            var result = new BadRequestObjectResult(new
            {
                Message = "Validation failed",
                Errors = errors
            });

            context.Result = result;
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
