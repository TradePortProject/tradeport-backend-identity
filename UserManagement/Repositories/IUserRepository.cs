
using System.Threading.Tasks;
using UserManagement.Data;
using UserManagement.Models;

namespace UserManagement.Repositories
{
    public interface IUserRepository : IRepositoryBase<User>
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIDAsync(Guid user);
        Task<bool> CreateUserAsync(User user); // Ensure it returns bool
        Task<bool> UpdateUserByIDAsync(Guid ID, User user);
    }
}

