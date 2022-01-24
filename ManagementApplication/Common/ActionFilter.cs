using Microsoft.AspNetCore.Mvc.Filters;

namespace ManagementApplication.Common
{
    public class ActionFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Do something before the action executes.
            var test = context;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Do something after the action executes.
            var test = context;
        }
    }
}
