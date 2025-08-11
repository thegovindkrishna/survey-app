using Survey.Models;

namespace Survey.Repositories
{
    public interface IUserRepository : IRepository<UserModel>
    {
        Task<UserModel?> GetUserByEmail(string email);
        Task<IEnumerable<UserModel>> GetAll();
        Task<UserModel?> GetById(int id);
        new Task Update(UserModel user);
        new Task Remove(UserModel user);
    }
}
