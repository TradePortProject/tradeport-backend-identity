using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Repositories;
using Xunit;

namespace UserManagement.Tests.Repositories
{
    public class UserRepositoryTests
    {
        private AppDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task CreateUserAsync_Should_Return_True_When_User_Created()
        {
            using var context = GetInMemoryContext();
            var repo = new UserRepository(context);

            var user = new User
            {
                UserID = Guid.NewGuid(),
                UserName = "Test User",
                loginID = "test@example.com",
                Role = 1,
                CreatedOn = DateTime.UtcNow,
                IsActive = true
            };

            var result = await repo.CreateUserAsync(user);
            Assert.True(result);
            Assert.Single(context.Users);
        }

        [Fact]
        public async Task GetUserByEmailAsync_Should_Return_User()
        {
            using var context = GetInMemoryContext();
            var user = new User
            {
                UserID = Guid.NewGuid(),
                UserName = "Test User",
                loginID = "email@example.com",
                Role = 1,
                IsActive = true
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var repo = new UserRepository(context);
            var result = await repo.GetUserByEmailAsync("email@example.com");

            Assert.NotNull(result);
            Assert.Equal("Test User", result.UserName);
        }
        [Fact]
        public async Task UpdateUserByIDAsync_Should_Update_User_And_Return_True()
        {
            using var context = GetInMemoryContext();
            var userId = Guid.NewGuid();

            // Create the initial user
            context.Users.Add(new User
            {
                UserID = userId,
                UserName = "Old Name",
                loginID = "old@example.com",
                Address = "Old Address",
                PhoneNo = "0000",
                Role = 1,
                IsActive = true,
                CreatedOn = DateTime.UtcNow,
                Password = new byte[] { 1 },
                StrPassword = null,
                Remarks = null
            });
            await context.SaveChangesAsync();

            var repo = new UserRepository(context);

            // Pass a valid object (even though only a few fields are updated)
            var updatedUser = new User
            {
                UserName = "New Name",
                Address = "New Address",
                PhoneNo = "9999",

                // Fill required fields to keep EF happy (even if not used)
                loginID = "old@example.com",
                Role = 1,
                IsActive = true,
                CreatedOn = DateTime.UtcNow,
                Password = new byte[] { 1 },
                StrPassword = null,
                Remarks = null
            };

            var result = await repo.UpdateUserByIDAsync(userId, updatedUser);

            Assert.True(result);
            var updated = await context.Users.FindAsync(userId);
            Assert.Equal("New Name", updated.UserName);
            Assert.Equal("New Address", updated.Address);
            Assert.Equal("9999", updated.PhoneNo);
        }


        [Fact]
        public async Task GetUserByIDAsync_Should_Return_Active_User()
        {
            using var context = GetInMemoryContext();
            var userId = Guid.NewGuid();

            context.Users.Add(new User
            {
                UserID = userId,
                UserName = "Active User",
                IsActive = true,
                Role = 1
            });

            context.Users.Add(new User
            {
                UserID = Guid.NewGuid(),
                UserName = "Inactive User",
                IsActive = false,
                Role = 2
            });

            await context.SaveChangesAsync();

            var repo = new UserRepository(context);
            var result = await repo.GetUserByIDAsync(userId);

            Assert.NotNull(result);
            Assert.Equal("Active User", result.UserName);
        }
    }
}
