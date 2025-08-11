using Survey.Models;

namespace Survey.Services
{
    public interface IUserService
    {
        Task<UserModel?> GetUserByEmail(string email);
        Task<IEnumerable<UserModel>> GetAllUsers();
        Task<bool> PromoteToAdmin(int userId);
        Task<bool> DeleteUser(int userId);
    }
}
