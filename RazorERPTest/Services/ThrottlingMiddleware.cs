using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RazorERPTest.Services
{
    public class ThrottlingMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly ConcurrentDictionary<string, UserRequestCount> _requests = new();
        private const int _requestLimit = 10; // Limit to 10 requests per minute
        private const int _timeWindowInSeconds = 60;
        private readonly IConfiguration _configuration;

        public ThrottlingMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var userId = context.User?.Identity?.Name ?? context.Connection.RemoteIpAddress.ToString();

            if (string.IsNullOrEmpty(userId))
            {
                await _next(context);
                return;
            }

            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
            {
                var userPrincipal = ValidateToken(token);
                if (userPrincipal != null)
                {
                    context.User = userPrincipal;
                }
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

        private ClaimsPrincipal ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:IssuerSigningKey"]);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                // Check if the token is a valid JWT token
                if (validatedToken is JwtSecurityToken jwtToken && jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return principal; // This principal is now authenticated
                }
            }
            catch
            {
                // Token validation failed
            }

            return null; // Return null if validation fails
        }
    }

    public class UserRequestCount
    {
        public int Count { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

}
