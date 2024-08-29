using Dapper;
using Microsoft.Data.SqlClient;
using RazorERPTest.Interfaces;
using RazorERPTest.Models;
using System.Data;

namespace RazorERPTest.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;
        public UserRepository(IConfiguration configuration) {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<int> CreateUserAsync(User user)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("Username", user.Username);
            parameters.Add("PasswordHash", user.PasswordHash);
            parameters.Add("Role", user.Role);
            parameters.Add("CompanyId", user.CompanyId);
            parameters.Add("IsActive", true);

            return await connection.ExecuteScalarAsync<int>(
                "spCreateUser", parameters, commandType: CommandType.StoredProcedure);

        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            var result = await connection.ExecuteAsync(
                "spDeleteUser", new { Id = id }, commandType: CommandType.StoredProcedure);
            return result > 0;
        }

        public async Task<IEnumerable<User>> GetNonAdminUsersByCompanyAsync(int companyId)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<User>(
                "spGetNonAdminUsersByCompany", new { CompanyId = companyId }, commandType: CommandType.StoredProcedure);
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<User>(
                    "spGetUserById", new { Id = id }, commandType: CommandType.StoredProcedure);
        }

        public async Task<User> GetUserByUsernameAsync(string userName)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE IsActive = 1 AND Username = @Username", new { Username = userName });

        }

        public async Task<IEnumerable<User>> GetUsersByCompanyAsync(int companyId)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<User>(
                "spGetUsersByCompany", new { CompanyId = companyId }, commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("Id", user.Id);
            parameters.Add("Username", user.Username);
            parameters.Add("PasswordHash", user.PasswordHash);
            parameters.Add("Role", user.Role);
            parameters.Add("CompanyId", user.CompanyId);

            var result = await connection.ExecuteAsync(
                "spUpdateUser", parameters, commandType: CommandType.StoredProcedure);
            return result > 0;
        }
    }
}
