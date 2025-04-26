using System;
using UserManagement.Models;
using Xunit;

namespace UserManagement.Tests.Models
{
    public class UserCredentialsTests
    {
        [Fact]
        public void Can_Create_UserCredentials_With_Valid_Data()
        {
            // Arrange
            var credentials = new UserCredentials
            {
                UserID = Guid.NewGuid(),
                Email = "sowjanya@example.com",
                Name = "Sowjanya",
                Role = 1
            };

            // Assert
            Assert.Equal("sowjanya@example.com", credentials.Email);
            Assert.Equal("Sowjanya", credentials.Name);
            Assert.Equal(1, credentials.Role);
            Assert.NotEqual(Guid.Empty, credentials.UserID);
        }

        [Fact]
        public void Can_Assign_Null_To_Email_And_Name()
        {
            var credentials = new UserCredentials
            {
                Email = null,
                Name = null
            };

            Assert.Null(credentials.Email);
            Assert.Null(credentials.Name);
        }
    }
}
