using RazorERPTest.Models;

namespace RazorERPTest.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetUserByIdAsync(int id);
        Task<User> GetUserByUsernameAsync(string userName);
        Task<IEnumerable<User>> GetUsersByCompanyAsync(int companyId);
        Task<IEnumerable<User>> GetNonAdminUsersByCompanyAsync(int companyId);
        Task<int> CreateUserAsync(User user);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
    }
}
