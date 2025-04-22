using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using UserManagement.Models;
using UserManagement.Services;
using Xunit;

namespace UserManagement.Tests.Services
{
    public class JwtServiceTests
    {
        private readonly IJwtService _jwtService;

        public JwtServiceTests()
        {
            // Set up test configuration
            var configValues = new Dictionary<string, string>
            {
                {"Jwt:Key", "TestSuperSecretKey12345678901234567890"},
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"},
                {"Google:ClientId", "fake-google-client-id.apps.googleusercontent.com"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configValues)
                .Build();

            var loggerMock = new Mock<ILogger<JwtService>>();

            _jwtService = new JwtService(configuration, loggerMock.Object);
        }

        [Fact]
        public void GenerateToken_ShouldReturn_NonEmptyToken()
        {
            // Arrange
            var user = new UserCredentials
            {
                UserID = Guid.NewGuid(),
                Name = "Test User",
                Email = "testuser@example.com",
                Role = 1
            };

            // Act
            var token = _jwtService.GenerateToken(user);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(token));
        }

        [Fact]
        public void GeneratedToken_ShouldContainExpectedClaims()
        {
            // Arrange
            var user = new UserCredentials
            {
                UserID = Guid.NewGuid(),
                Name = "Test User",
                Email = "testuser@example.com",
                Role = 1
            };

            var token = _jwtService.GenerateToken(user);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Assert
            Assert.Equal("TestIssuer", jwtToken.Issuer);
            Assert.Equal("TestAudience", jwtToken.Audiences.First());
            Assert.Contains(jwtToken.Claims, c => c.Type == ClaimTypes.Email && c.Value == user.Email);
            Assert.Contains(jwtToken.Claims, c => c.Type == ClaimTypes.Name && c.Value == user.Name);
            Assert.Contains(jwtToken.Claims, c => c.Type == ClaimTypes.Role && c.Value == user.Role.ToString());
        }

        [Fact]
        public async Task ValidateToken_InvalidToken_ShouldReturnNull()
        {
            // Act
            var result = await _jwtService.ValidateToken("this-is-not-a-valid-google-token");

            // Assert
            Assert.Null(result); // Should gracefully handle the exception and return null
        }
    }
}
