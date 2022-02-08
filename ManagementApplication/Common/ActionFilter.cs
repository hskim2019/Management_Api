using Management_Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.Text.Json;

namespace ManagementApplication.Common
{
    public class ActionFilter : IActionFilter//, IOrderedFilter
    {
        //public int Order => int.MaxValue - 10; // Order allows other filters to run at the end of the pipeline.
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Do something before the action executes.
            var test = context;
        }
        
        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Do something after the action executes.
            var test = context;

            // Exception handling -> ExceptionHandlingMiddlewareExtensions
            //if (context.Exception is HttpResponseException httpResponseException)
            //{
            //    context.Result = new ObjectResult(httpResponseException.Value)
            //    {
            //        StatusCode = httpResponseException.StatusCode
            //    };

            //    context.ExceptionHandled = true;
            //}

        }
    }
}
