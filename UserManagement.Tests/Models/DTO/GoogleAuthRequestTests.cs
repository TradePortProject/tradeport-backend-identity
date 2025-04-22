using System;
using UserManagement.Models.DTO;
using Xunit;

namespace UserManagement.Tests.Models
{
    public class GoogleAuthRequestTests
    {
        [Fact]
        public void Can_Create_GoogleAuthRequest_With_Token()
        {
            // Arrange
            var tokenValue = "test-google-oauth-token";

            // Act
            var request = new GoogleAuthRequest
            {
                Token = tokenValue
            };

            // Assert
            Assert.Equal(tokenValue, request.Token);
        }

        [Fact]
        public void Token_Property_Should_Allow_Null()
        {
            // Act
            var request = new GoogleAuthRequest
            {
                Token = null
            };

            // Assert
            Assert.Null(request.Token);
        }

        [Fact]
        public void Token_Property_Should_Allow_EmptyString()
        {
            // Act
            var request = new GoogleAuthRequest
            {
                Token = ""
            };

            // Assert
            Assert.Equal(string.Empty, request.Token);
        }
    }
}
