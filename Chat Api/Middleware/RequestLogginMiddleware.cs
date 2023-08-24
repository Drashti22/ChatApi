using System.Text;
using Chat_Api.Models;
using Microsoft.EntityFrameworkCore;
using Chat_Api.Context;
using EcmaScript.NET;
using System.Security.Claims;

namespace Chat_Api.Middleware
{
    public class RequestLogginMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLogginMiddleware> _logger;

        public RequestLogginMiddleware(RequestDelegate next, ILogger<RequestLogginMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task Invoke(HttpContext context, AppDbContext dbContext)
        {
            var request = context.Request;
            var requestBody = await GetRequestBody(request);


            var log = new Log
            {
                IpAddress = GetIpAddress(context),
                RequestBody = requestBody,
                TimeStamp = DateTime.Now,
                
            };

            dbContext.logs.Add(log);
            await dbContext.SaveChangesAsync();

            await _next(context);
        }

        private async Task<string> GetRequestBody(HttpRequest request)
        {
            request.EnableBuffering();
            var body = string.Empty;

            using (var reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
            {
                body = await reader.ReadToEndAsync();
                request.Body.Position = 0;
            }

            return body;
        }

        private string GetIpAddress(HttpContext context)
        {
            return context.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        }

       

    }

    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLoggingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLogginMiddleware>();
        }
    }
}


