using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace ManagementApplication.Common
{
    public class HttpAppAuthorizationService : DefaultAuthorizationService, IAuthorizationService
    {
        readonly IHttpContextAccessor _httpContextAccessor;
        public HttpAppAuthorizationService(IAuthorizationPolicyProvider policyProvider,
            IAuthorizationHandlerProvider handlers,
            ILogger<DefaultAuthorizationService> logger,
            IAuthorizationHandlerContextFactory contextFactory,
            IAuthorizationEvaluator evaluator,
            IOptions<AuthorizationOptions> options,
            IHttpContextAccessor httpContextAccessor) : base(policyProvider, handlers, logger, contextFactory, evaluator, options)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        async Task<AuthorizationResult> IAuthorizationService.AuthorizeAsync(ClaimsPrincipal user, object resource, IEnumerable<IAuthorizationRequirement> requirements)
        {
            var result = await base.AuthorizeAsync(user, resource, requirements);
            //capture the result for later using
            _setAuthorizationResultFeature(result);
            return result;
        }
        async Task<AuthorizationResult> IAuthorizationService.AuthorizeAsync(ClaimsPrincipal user, object resource, string policyName)
        {
            var result = await base.AuthorizeAsync(user, resource, policyName);
            //capture the result for later using
            _setAuthorizationResultFeature(result);
            return result;
        }
        void _setAuthorizationResultFeature(AuthorizationResult result)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                httpContext.Features.Set<IAuthorizationResultFeature>(new AuthorizationResultFeature(result));
            }
        }
    }

    public interface IAuthorizationResultFeature
    {
        AuthorizationResult AuthorizationResult { get; }
    }
    public class AuthorizationResultFeature : IAuthorizationResultFeature
    {
        public AuthorizationResultFeature(AuthorizationResult result)
        {
            AuthorizationResult = result;
        }
        public AuthorizationResult AuthorizationResult { get; }
    }
}
