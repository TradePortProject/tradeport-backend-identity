using System;
using UserManagement.Models;
using Xunit;

namespace UserManagement.Tests.Models
{
    public class GoogleUserTests
    {
        [Fact]
        public void Can_Create_GoogleUser_With_Valid_Data()
        {
            // Arrange
            var user = new GoogleUser
            {
                Sub = "1234567890",
                Email = "sowjanya@example.com",
                Name = "Sowjanya",
                Picture = "https://example.com/pic.jpg",
                EmailVerified = true
            };

            // Assert
            Assert.Equal("1234567890", user.Sub);
            Assert.Equal("sowjanya@example.com", user.Email);
            Assert.Equal("Sowjanya", user.Name);
            Assert.Equal("https://example.com/pic.jpg", user.Picture);
            Assert.True(user.EmailVerified);
        }

        [Fact]
        public void Can_Create_GoogleUser_With_Null_Optional_Values()
        {
            // Arrange
            var user = new GoogleUser
            {
                Sub = null,
                Email = "test@example.com",
                Name = "Test User",
                Picture = null,
                EmailVerified = false
            };

            // Assert
            Assert.Null(user.Sub);
            Assert.Null(user.Picture);
            Assert.Equal("test@example.com", user.Email);
            Assert.Equal("Test User", user.Name);
            Assert.False(user.EmailVerified);
        }
    }
}
