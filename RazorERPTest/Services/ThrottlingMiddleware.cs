using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace RazorERPTest.Services
{
    public class ThrottlingMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly ConcurrentDictionary<string, UserRequestCount> _requests = new();
        private const int _requestLimit = 10; // Limit to 10 requests per minute
        private const int _timeWindowInSeconds = 60;

        public ThrottlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var userId = context.User?.Identity?.Name ?? context.Connection.RemoteIpAddress.ToString();

            if (string.IsNullOrEmpty(userId))
            {
                await _next(context);
                return;
            }

            var userRequestCount = _requests.GetOrAdd(userId, new UserRequestCount());

            lock (userRequestCount)
            {
                if (userRequestCount.Timestamp.AddSeconds(_timeWindowInSeconds) < DateTime.UtcNow)
                {
                    // Reset the count after the time window
                    userRequestCount.Count = 0;
                    userRequestCount.Timestamp = DateTime.UtcNow;
                }

                userRequestCount.Count++;

                if (userRequestCount.Count > _requestLimit)
                {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    return;
                }
            }

            await _next(context);
        }
    }

    public class UserRequestCount
    {
        public int Count { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

}
