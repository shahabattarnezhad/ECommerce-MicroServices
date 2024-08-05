using Ecommerce.SharedLibrary.Loggings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace Ecommerce.SharedLibrary.Middlewares;

public class GlobalException(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Declare default variables:
        string message = "Internal server error, try again!";

        int statusCode = (int)HttpStatusCode.InternalServerError;

        string title = "Error";

        try
        {
            await next(context);

            // Check here if the response is too many requests:  429
            if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
            {
                title = "Warning";
                message = "Too many requests";
                statusCode = (int)StatusCodes.Status429TooManyRequests;

                await ModifyHeader(context, title, message, statusCode);
            }

            // Check if the response is Unauthorized: 401
            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
            {
                title = "Alert";
                message = "You are not authorized.";
                statusCode = StatusCodes.Status401Unauthorized;

                await ModifyHeader(context, title, message, statusCode);
            }

            // Check if the response is Forbidden: 403
            if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
            {
                title = "Out of Access";
                message = "You do not have any access.";
                statusCode = StatusCodes.Status403Forbidden;

                await ModifyHeader(context, title, message, statusCode);
            }
        }
        catch (Exception ex)
        {
            // Log original exception:  --> File, Debugger, Console
            LogException.LogExceptions(ex);

            // Check if the exception is a 'timeout' exception:
            if (ex is TaskCanceledException || ex is TimeoutException)
            {
                title = "Timeout";
                message = "Request Timeout! Try again..";
                statusCode = StatusCodes.Status408RequestTimeout;
            }

            // If none of the above exceptions, then default:
            await ModifyHeader(context, title, message, statusCode);
        }
    }

    private static async Task ModifyHeader(HttpContext context, string title, string message, int statusCode)
    {
        // Display message to client:
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(new ProblemDetails
        {
            Detail = message,
            Status = statusCode,
            Title = title,
        }), CancellationToken.None);

        return;
    }
}
