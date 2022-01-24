using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace ManagementApplication.Common
{
    public class CustomAuthorizeFilter : ActionFilterAttribute, IAsyncAuthorizationFilter, IAsyncAlwaysRunResultFilter
    {

        public AuthorizationPolicy Policy { get; }

        public CustomAuthorizeFilter()
        {
            Policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Allow Anonymous skips all authorization
            bool hasAllowAnonymous = context.ActionDescriptor.EndpointMetadata
                                 .Any(em => em.GetType() == typeof(AllowAnonymousAttribute)); //< -- Here it is
            var endpoint = context.HttpContext.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                return;
            }

            // Allow Anonymous skips all authorization - not working
            //if (context.Filters.Any(item => item is IAllowAnonymousFilter))
            //{
            //    return;
            //}

            var policyEvaluator = context.HttpContext.RequestServices.GetRequiredService<IPolicyEvaluator>();
            var authenticateResult = await policyEvaluator.AuthenticateAsync(Policy, context.HttpContext);
            var authorizeResult = await policyEvaluator.AuthorizeAsync(Policy, authenticateResult, context.HttpContext, context);

            if (authorizeResult.Challenged)
            {
                // Return custom 401 result
                context.Result = new JsonResult(new
                {
                    Message = "Token Validation Has Failed. Request Access Denied"
                })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }

        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var msg = "";
            var authorizationResult = context.HttpContext.Features.Get<IAuthorizationResultFeature>()?.AuthorizationResult;
            if (authorizationResult?.Failure != null)
            {
                var rolesRequirements = authorizationResult.Failure.FailedRequirements.OfType<RolesAuthorizationRequirement>();
                msg = $@"You need to have all following roles (each group requires at least one role): 
                     {string.Join(", ", rolesRequirements.Select(e => $"({string.Join(", ", e.AllowedRoles)})"))}";
                //sends back a plain text result containing the msg
                //this can be obtained by the client
                context.Result = new ContentResult { Content = msg, StatusCode = 403 };
            }
            await next();
        }

    }

}