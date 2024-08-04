using Microsoft.AspNetCore.Http;

namespace Ecommerce.SharedLibrary.Middlewares;

public class ListenToOnlyApiGateway(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Extract specific header from the request:
        var signedHeader = context.Request.Headers["Api-Gateway"];

        // if null, it means the request is not coming from the Api Gateway:
        if(signedHeader.FirstOrDefault() is null)
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;

            await context.Response.WriteAsync("Service is unavailable!");

            return;
        }
        else
        {
            await next(context);
        }
    }
}
