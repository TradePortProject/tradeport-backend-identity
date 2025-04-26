using System;
using UserManagement.Models;
using Xunit;

namespace UserManagement.Tests.Models
{
    public class UserTests
    {
        [Fact]
        public void Can_Create_User_With_All_Properties_Set()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var user = new User
            {
                UserID = Guid.NewGuid(),
                loginID = "sow@example.com",
                UserName = "Sowjanya",
                Password = new byte[] { 1, 2, 3 },
                StrPassword = "c2VjcmV0",
                Role = 1,
                PhoneNo = "12345678",
                Address = "Singapore",
                Remarks = "Test user",
                CreatedOn = now,
                IsActive = true
            };

            // Assert
            Assert.Equal("sow@example.com", user.loginID);
            Assert.Equal("Sowjanya", user.UserName);
            Assert.Equal(new byte[] { 1, 2, 3 }, user.Password);
            Assert.Equal("c2VjcmV0", user.StrPassword);
            Assert.Equal(1, user.Role);
            Assert.Equal("12345678", user.PhoneNo);
            Assert.Equal("Singapore", user.Address);
            Assert.Equal("Test user", user.Remarks);
            Assert.Equal(now, user.CreatedOn);
            Assert.True(user.IsActive);
        }

        [Fact]
        public void Can_Assign_Null_Values_To_Optional_Fields()
        {
            var user = new User
            {
                UserID = Guid.NewGuid(),
                loginID = "test@example.com",
                UserName = "Test User",
                Role = 2,
                Password = null,
                StrPassword = null,
                PhoneNo = null,
                Address = null,
                Remarks = null,
                CreatedOn = null,
                IsActive = null
            };

            Assert.Null(user.Password);
            Assert.Null(user.StrPassword);
            Assert.Null(user.PhoneNo);
            Assert.Null(user.Address);
            Assert.Null(user.Remarks);
            Assert.Null(user.CreatedOn);
            Assert.Null(user.IsActive);
        }
    }
}
