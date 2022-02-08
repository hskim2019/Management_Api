
using System.Net;
using System.Security.Claims;
using System.Text;
using ErrorHandling.Api.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace ErrorHandling.Api.Extensions
{
    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseNativeGlobalExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var errorFeature = context.Features.Get<IExceptionHandlerFeature>();
                    var exception = errorFeature.Error;

                    // Log exception and/or run some other necessary code...

                    var errorResponse = new ErrorResponse();

                    if (exception is HttpException httpException)
                    {
                        errorResponse.StatusCode = (HttpStatusCode) httpException.StatusCode;
                        errorResponse.Message = httpException.Message;
                    }

                    //var errorMessage = exception.Message;
                    //var statusCode = (int)errorResponse.StatusCode;
                    //string requestUrl = context.Request.Path.HasValue ? context.Request.Path.Value : string.Empty;
                    //string queryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty;
                    //string userID = string.Empty;
                    //try { userID = context.User.Claims.FirstOrDefault(t => t.Type == ClaimTypes.NameIdentifier).Value; } catch (Exception){ }

                    context.Response.StatusCode = (int)errorResponse.StatusCode;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(errorResponse.ToJsonString());
                });
            });

            return app;
        }
    }
}