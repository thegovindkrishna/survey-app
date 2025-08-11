using Microsoft.EntityFrameworkCore;
using Survey.Data;
using Survey.Models;

namespace Survey.Repositories
{
    public class UserRepository : Repository<UserModel>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }

        public async new Task Update(UserModel user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async new Task Remove(UserModel user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<UserModel?> GetById(int id)
        {
            var user = await _context.Users.FindAsync(id);
            return user;
        }

        public async Task<UserModel?> GetUserByEmail(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            return user;
        }

        public async Task<IEnumerable<UserModel>> GetAll()
        {
            return await _context.Users.ToListAsync();
        }
    }
}
