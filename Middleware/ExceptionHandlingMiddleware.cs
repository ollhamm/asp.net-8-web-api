using System.Text.Json;
using aspnetcoreapi.Common;

namespace aspnetcoreapi.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        public ExceptionHandlingMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var (status, title) = ex switch
                {
                    UnauthorizedAccessException => (401, "Unauthorized"),
                    KeyNotFoundException => (404, "Not Found"),
                    InvalidOperationException => (409, "Conflict"),
                    ArgumentException or ArgumentNullException => (400, "Bad Request"),
                    _ => (500, "Internal Server Error")
                };

                var response = new ApiResponse
                {
                    Title = title,
                    Status = status,
                    Message = ex.Message
                };

                context.Response.StatusCode = status;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }
}