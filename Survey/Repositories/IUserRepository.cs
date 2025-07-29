
using Survey.Models;

namespace Survey.Repositories
{
    public interface IUserRepository : IRepository<UserModel>
    {
        Task<UserModel> GetUserByEmail(string email);
    }
}
