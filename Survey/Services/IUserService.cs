using Survey.Models;

namespace Survey.Services
{
    public interface IUserService
    {
        Task<UserModel> GetUserByEmail(string email);
    }
}
