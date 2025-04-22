
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;
using UserManagement.Models;
using Xunit;

namespace UserManagement.Tests.Repositories
{
    // Stub class for testing abstract RepositoryBase<T>
    public class UserRepositoryTestStub : RepositoryBase<User>
    {
        public UserRepositoryTestStub(AppDbContext context) : base(context) { }
    }

    public class RepositoryBaseTests
    {
        private AppDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task FindAll_Should_Return_All_Users()
        {
            // Arrange
            using var context = GetInMemoryContext();
            context.Users.AddRange(
                new User { UserID = Guid.NewGuid(), UserName = "User1", loginID = "user1@test.com", Role = 1 },
                new User { UserID = Guid.NewGuid(), UserName = "User2", loginID = "user2@test.com", Role = 2 }
            );
            await context.SaveChangesAsync();

            var repo = new UserRepositoryTestStub(context);

            // Act
            var result = repo.FindAll().ToList();

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task FindByCondition_Should_Return_Filtered_Users()
        {
            // Arrange
            using var context = GetInMemoryContext();
            context.Users.AddRange(
                new User { UserID = Guid.NewGuid(), UserName = "Admin", loginID = "admin@test.com", Role = 1 },
                new User { UserID = Guid.NewGuid(), UserName = "Guest", loginID = "guest@test.com", Role = 2 }
            );
            await context.SaveChangesAsync();

            var repo = new UserRepositoryTestStub(context);

            // Act
            var admins = repo.FindByCondition(u => u.Role == 1).ToList();

            // Assert
            Assert.Single(admins);
            Assert.Equal("Admin", admins[0].UserName);
        }
    }
}
