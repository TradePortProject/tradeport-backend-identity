
using System.Threading.Tasks;
using UserManagement.Models;

namespace UserManagement.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserByEmailAsync(string email);
        Task<bool> CreateUserAsync(User user); // Ensure it returns bool
    }
}

