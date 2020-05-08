using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CW2.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }


        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                var httpMethod = httpContext.Request.Method;
                var path = httpContext.Request.Path;
                var queryString = httpContext.Request.QueryString;
                string requestBody;
                httpContext.Request.EnableBuffering();
                using (var reader = new StreamReader(httpContext.Request.Body, leaveOpen: true))
                {
                    requestBody = await reader.ReadToEndAsync();
                    httpContext.Request.Body.Seek(0, SeekOrigin.Begin);
                }
                File.AppendAllLines("requestsLog.txt", new[] { $"{httpMethod} {path} {queryString} {requestBody}" });
            }
            catch (Exception e)
            {
                Console.WriteLine("cannot log request" + e);
            }
            await _next(httpContext);
        }
    }
}
