using RazorERPTest.Models;

namespace RazorERPTest.Interfaces
{
    public interface IAuthService
    {
        Task<string> AuthenticateAsync(LoginRequest loginRequest);

    }
}
