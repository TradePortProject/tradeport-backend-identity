using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;
using UserManagement.Models;
using Xunit;

namespace UserManagement.Tests.Data
{
    public class AppDbContextTests
    {
        private DbContextOptions<AppDbContext> GetInMemoryOptions()
        {
            return new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;
        }

        [Fact]
        public void Can_Create_DbContext_And_Access_UsersDbSet()
        {
            var options = GetInMemoryOptions();
            using var context = new AppDbContext(options);

            Assert.NotNull(context.Users);
        }

        [Fact]
        public async Task Can_Insert_And_Retrieve_User()
        {
            var options = GetInMemoryOptions();

            // Insert
            using (var context = new AppDbContext(options))
            {
                var user = new User
                {
                    UserID = Guid.NewGuid(),
                    UserName = "Sowjanya",
                    loginID = "sow@example.com",
                    Password = Convert.FromBase64String("dGVzdHBhc3M="),
                    Role = 1,
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow
                };

                context.Users.Add(user);
                await context.SaveChangesAsync();
            }

            // Retrieve
            using (var context = new AppDbContext(options))
            {
                var savedUser = await context.Users
                    .FirstOrDefaultAsync(u => u.loginID == "sow@example.com");

                Assert.NotNull(savedUser);
                Assert.Equal("Sowjanya", savedUser.UserName);
                Assert.Equal(1, savedUser.Role);
            }
        }

        [Fact]
        public void OnModelCreating_Should_Map_Users_To_CorrectTableName()
        {
            var options = GetInMemoryOptions();
            using var context = new AppDbContext(options);
            var entityType = context.Model.FindEntityType(typeof(User));

            var tableName = entityType.GetTableName();
            Assert.Equal("Users", tableName); // Validates the ToTable("Users") mapping
        }
    }
}
