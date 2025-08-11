using Survey.Models;
using Survey.Repositories;
using Microsoft.Extensions.Logging;

namespace Survey.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserService> _logger;

        public UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<UserModel?> GetUserByEmail(string email)
        {
            return await _unitOfWork.Users.GetUserByEmail(email);
        }

        public async Task<IEnumerable<UserModel>> GetAllUsers()
        {
            return await _unitOfWork.Users.GetAll();
        }

        public async Task<bool> PromoteToAdmin(int userId)
        {
            var user = await _unitOfWork.Users.GetById(userId);
            if (user == null) return false;
            user.Role = "Admin";
            await _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> DeleteUser(int userId)
        {
            var user = await _unitOfWork.Users.GetById(userId);
            if (user == null) return false;
            await _unitOfWork.Users.Remove(user);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
