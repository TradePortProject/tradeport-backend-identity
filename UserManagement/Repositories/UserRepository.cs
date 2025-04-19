
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UserManagement.Data;
using UserManagement.Models;

namespace UserManagement.Repositories
{
    public class UserRepository : RepositoryBase<User>, IUserRepository
    {
        private readonly AppDbContext _dbContext;


        public UserRepository(AppDbContext dbContextRepo) : base(dbContextRepo)
        {
            _dbContext = dbContextRepo;
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.loginID == email);
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            try
            {
                await _dbContext.Users.AddAsync(user);
                await _dbContext.SaveChangesAsync();
                return true; // Return true if user creation is successful
            }
            catch (Exception)
            {
                return false; // Return false if an error occurs
            }
        }


        public async Task<bool> UpdateUserByIDAsync(Guid ID, User user)
        {
            int result = 0;
            var userObj = await _dbContext.Users.FindAsync(ID);

            if (userObj != null)
            {
                userObj.UserName = user.UserName;
                userObj.Address = user.Address;
                userObj.PhoneNo = user.PhoneNo;;
                result = await _dbContext.SaveChangesAsync();
            }

            return result > 0;
        }

        public async Task<User?> GetUserByIDAsync(Guid userID)
        {
            return await FindByCondition(user => user.UserID == userID && user.IsActive == true).FirstOrDefaultAsync();
        }

    }
}
