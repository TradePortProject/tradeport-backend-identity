using Xunit;
using Moq;
using UserManagement.Controllers;
using UserManagement.Services;
using UserManagement.Repositories;
using UserManagement.Models;
using UserManagement.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using System.Threading.Tasks;
using System;
using UserManagement.Data;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;


namespace UserManagement.Tests.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IJwtService> _mockJwtService;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _mockJwtService = new Mock<IJwtService>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockMapper = new Mock<IMapper>();

            _controller = new UserController(
                _mockJwtService.Object,
                _mockUserRepository.Object,
                new Mock<IConfiguration>().Object,
                null,//new Mock<AppDbContext>().Object,
                _mockMapper.Object
            );
        }


        [Fact]
        public async Task ValidateGoogleUser_WithValidTokenAndExistingUser_ReturnsOkWithToken()
        {
            // Arrange
            var fakeToken = "valid-google-token";
            var googleUser = new GoogleUser
            {
                Email = "test@example.com",
                Name = "Test User"
            };

            var user = new User
            {
                UserID = Guid.NewGuid(),
                UserName = "Test User",
                loginID = "test@example.com",
                Role = 1
            };

            var userDto = new UserDTOAuth
            {
                Email = user.loginID,
                UserName = user.UserName,
                Role = user.Role
            };

            var googleAuthRequest = new GoogleAuthRequest { Token = fakeToken };  // Create GoogleAuthRequest object

            _mockJwtService.Setup(x => x.ValidateToken(fakeToken)).ReturnsAsync(googleUser); // Simulate valid Google token
            _mockUserRepository.Setup(x => x.GetUserByEmailAsync(googleUser.Email)).ReturnsAsync(user); // Get user by email
            _mockMapper.Setup(x => x.Map<UserDTOAuth>(user)).Returns(userDto); // Map user to DTO
            _mockJwtService.Setup(x => x.GenerateToken(It.IsAny<UserCredentials>())).Returns("mock-jwt-token"); // Generate token

            // Act
            var result = await _controller.ValidateGoogleUser(googleAuthRequest);  // Pass GoogleAuthRequest object

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result); // Assert OK result type
            Assert.NotNull(okResult.Value);
            var json = JObject.FromObject(okResult.Value); // Parse the result value

            // Extract values from response         
            string token = json["Token"]?.ToString() ?? "";  // Assign an empty string if null
            string email = json["User"]?["Email"]?.ToString() ?? "";  // Assign an empty string if null


            Assert.Equal("mock-jwt-token", token); // Assert the generated token
            Assert.Equal("test@example.com", email); // Assert the user email
        }



        [Fact]
        public async Task ValidateGoogleUser_WithInvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            var invalidToken = "invalid-google-token";

            _mockJwtService
                .Setup(service => service.ValidateToken(invalidToken))
                .ReturnsAsync((GoogleUser?)null); // Simulate invalid token

            var googleAuthRequest = new GoogleAuthRequest { Token = invalidToken }; // Create GoogleAuthRequest object

            // Act
            var result = await _controller.ValidateGoogleUser(googleAuthRequest); // Pass GoogleAuthRequest object

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);

            // Ensure the response is not null before accessing it
            Assert.NotNull(unauthorizedResult.Value);

            // Check if the response is a string and compare it directly
            var responseMessage = unauthorizedResult.Value.ToString();
            Assert.Equal("Invalid Google token.", responseMessage); // Compare message value
        }



        [Fact]
        public async Task ValidateGoogleUser_WithValidTokenButUserNotFound_ReturnsNotFound()
        {
            // Arrange
            var token = "valid-token-no-user";

            var googleUser = new GoogleUser
            {
                Email = "nouser@example.com",
                Name = "No User"
            };

            // Mock the service to return Google user
            _mockJwtService
                .Setup(service => service.ValidateToken(token))
                .ReturnsAsync(googleUser);

            // Mock user repository to return null (user not found)
            _mockUserRepository
                .Setup(repo => repo.GetUserByEmailAsync(googleUser.Email))
                .ReturnsAsync((User?)null); // Explicitly return null (nullable)

            var googleAuthRequest = new GoogleAuthRequest { Token = token }; // Create GoogleAuthRequest object

            // Act
            var result = await _controller.ValidateGoogleUser(googleAuthRequest); // Pass GoogleAuthRequest object

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result); // Expect NotFoundObjectResult

            // Check if the response is a string (not a complex object)
            Assert.IsType<string>(notFoundResult.Value);

            // Compare the error message string returned from the NotFoundObjectResult
            Assert.Equal("User does not exist in the database.", notFoundResult.Value);
        }





        [Fact]
        public async Task RegisterUser_WithValidNewUser_ReturnsOk()
        {
            // Arrange
            var newUser = new User
            {
                loginID = "newuser@example.com",
                StrPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes("SecurePass123")),
                UserName = "New User"
            };

            // Simulate user not found
            _mockUserRepository
                .Setup(repo => repo.GetUserByEmailAsync(newUser.loginID))
                .ReturnsAsync((User?)null);

            // Simulate successful creation
            _mockUserRepository
                .Setup(repo => repo.CreateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(true);


            // Act
            var result = await _controller.RegisterUser(newUser);


            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("User registered successfully.", okResult.Value);

        }

        [Fact]
        public async Task RegisterUser_WithMissingLoginID_ReturnsBadRequest()
        {
            // Arrange
            var newUser = new User
            {
                loginID = null, // Missing login ID
                StrPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes("AnyPassword")),
                UserName = "NoLoginIdUser"
            };


            // Act
            var result = await _controller.RegisterUser(newUser);


            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid user data.", badRequestResult.Value);

        }

        [Fact]
        public async Task RegisterUser_WithInvalidBase64Password_ReturnsBadRequest()
        {
            // Arrange
            var newUser = new User
            {
                loginID = "invalidbase64@example.com",
                StrPassword = "NotBase64!!!", // invalid base64 string
                UserName = "InvalidBase64User"
            };


            // Act
            var result = await _controller.RegisterUser(newUser);


            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid Base64 format for password.", badRequestResult.Value);

        }

        [Fact]
        public async Task RegisterUser_WhenUserAlreadyExists_ReturnsBadRequest()
        {
            // Arrange
            var newUser = new User
            {
                loginID = "existinguser@example.com",
                StrPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes("SomePassword")),
                UserName = "Existing User"
            };

            var existingUser = new User
            {
                loginID = "existinguser@example.com"
            };

            _mockUserRepository
                .Setup(repo => repo.GetUserByEmailAsync(newUser.loginID))
                .ReturnsAsync(existingUser);


            // Act
            var result = await _controller.RegisterUser(newUser);


            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("User already exists.", badRequestResult.Value);

        }

        [Fact]
        public async Task RegisterUser_WhenSaveFails_ReturnsStatusCode500()
        {
            // Arrange
            var newUser = new User
            {
                loginID = "newuserfail@example.com",
                StrPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes("SecurePassword")),
                UserName = "SaveFailUser"
            };

            _mockUserRepository
                .Setup(repo => repo.GetUserByEmailAsync(newUser.loginID))
                .ReturnsAsync((User?)null); // User doesn't exist

            _mockUserRepository
                .Setup(repo => repo.CreateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(false); // Simulate save failure


            // Act
            var result = await _controller.RegisterUser(newUser);


            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);

        }

        [Fact]
        public async Task UpdateUserByID_WithValidUserIDAndDTO_ReturnsOk()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userDto = new UserDTO
            {
                UserName = "Updated User",
                PhoneNo = "1234567890",
                Address = "123 New Address"
            };

            var existingUser = new User
            {
                UserID = userId,
                UserName = "Old User",
                loginID = "olduser@example.com", // If needed for other tests
                Role = 1
            };

            // Mock GetUserByIDAsync to return the existing user
            _mockUserRepository
                .Setup(repo => repo.GetUserByIDAsync(userId))
                .ReturnsAsync(existingUser);

            // Mock UpdateUserByIDAsync to simulate a successful update
            _mockUserRepository
                .Setup(repo => repo.UpdateUserByIDAsync(userId, It.IsAny<User>()))
                .ReturnsAsync(true);



            // Act
            var result = await _controller.UpdateUserByID(userId, userDto);


            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = JObject.FromObject(okResult.Value);

            Assert.Equal("User information updated successfully.", response["Message"].ToString());


        }


        [Fact]
        public async Task UpdateUserByID_WhenUserNotFound_ReturnsNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid(); // Non-existing user ID
            var userDto = new UserDTO
            {
                UserName = "Updated User",
                PhoneNo = "1234567890",
                Address = "Updated Address"
            };

            // Mock GetUserByIDAsync to return null (user not found)
            _mockUserRepository
                .Setup(repo => repo.GetUserByIDAsync(userId))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _controller.UpdateUserByID(userId, userDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = JObject.FromObject(notFoundResult.Value); // Using JObject to access the response

            Assert.Equal("User not found.", response["Message"].ToString()); // Access the 'Message' property safely
        }

        [Fact]
        public async Task UpdateUserByID_WhenUpdateFails_ReturnsStatusCode500()
        {
            // Arrange
            var userId = Guid.NewGuid(); // Non-existing user ID
            var userDto = new UserDTO
            {
                UserName = "Updated User",
                PhoneNo = "1234567890",
                Address = "Updated Address"
            };

            var existingUser = new User
            {
                UserID = userId,
                UserName = "Old User",
                loginID = "olduser@example.com", // Existing user
                Role = 1
            };

            // Mock GetUserByIDAsync to return the existing user
            _mockUserRepository
                .Setup(repo => repo.GetUserByIDAsync(userId))
                .ReturnsAsync(existingUser);

            // Mock UpdateUserByIDAsync to simulate a failure (returns false)
            _mockUserRepository
                .Setup(repo => repo.UpdateUserByIDAsync(userId, It.IsAny<User>()))
                .ReturnsAsync(false); // Simulate update failure

            // Act
            var result = await _controller.UpdateUserByID(userId, userDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var response = JObject.FromObject(statusCodeResult.Value); // Using JObject to access properties
            Assert.Equal("Failed to update user information.", response["Message"].ToString());
        }

        [Fact]
        public async Task UpdateUserByID_WhenDTOIsInvalid_ReturnsBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid(); // Valid user ID
            var userDto = new UserDTO
            {
                UserName = "Updated User",
                PhoneNo = "1234567890",
                Address = "Updated Address"
            };

            var existingUser = new User
            {
                UserID = userId,
                UserName = "Old User",
                loginID = "olduser@example.com", // Existing user
                Role = 1
            };

            // Mock GetUserByIDAsync to return the existing user
            _mockUserRepository
                .Setup(repo => repo.GetUserByIDAsync(userId))
                .ReturnsAsync(existingUser);

            // Mock UpdateUserByIDAsync to simulate a failure (returns false)
            _mockUserRepository
                .Setup(repo => repo.UpdateUserByIDAsync(userId, It.IsAny<User>()))
                .ReturnsAsync(false); // Simulate update failure

            // Act
            var result = await _controller.UpdateUserByID(userId, userDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode); // Assert that it's a 500 StatusCode
            var response = JObject.FromObject(statusCodeResult.Value); // Using JObject to access properties
            Assert.Equal("Failed to update user information.", response["Message"].ToString()); // Check the Message property
            Assert.Equal("Internal server error.", response["ErrorMessage"].ToString()); // Check the ErrorMessage property
        }

        //[Fact]
        //public async Task UpdateUserByID_WhenNotAuthenticated_ReturnsUnauthorized()
        //{
        //    // Arrange
        //    var userId = Guid.NewGuid(); // Valid user ID
        //    var userDto = new UserDTO
        //    {
        //        UserName = "Updated User",
        //        PhoneNo = "1234567890",
        //        Address = "Updated Address"
        //    };

        //    // Simulate an unauthenticated request by not setting authentication
        //    var httpContext = new DefaultHttpContext();
        //    httpContext.Request.Headers["Authorization"] = ""; // Empty Authorization header

        //    // Assign this to the controller's context
        //    _controller.ControllerContext = new ControllerContext()
        //    {
        //        HttpContext = httpContext
        //    };

        //    // Act
        //    var result = await _controller.UpdateUserByID(userId, userDto);

        //    // Assert
        //    var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result); // Expect UnauthorizedResult
        //    Assert.Equal(401, unauthorizedResult.StatusCode); // Ensure the status code is 401
        //}

        [Fact]
        public async Task UpdateUserByID_WhenDatabaseFails_ReturnsStatusCode500()
        {
            var userId = Guid.NewGuid();
            var userDto = new UserDTO
            {
                UserName = "Updated User",
                PhoneNo = "1234567890",
                Address = "Updated Address"
            };

            var existingUser = new User
            {
                UserID = userId,
                UserName = "Old User",
                loginID = "olduser@example.com",
                Role = 1
            };

            _mockUserRepository
                .Setup(repo => repo.GetUserByIDAsync(userId))
                .ReturnsAsync(existingUser);
            _mockUserRepository
                .Setup(repo => repo.UpdateUserByIDAsync(userId, It.IsAny<User>()))
                .ReturnsAsync(false); // Simulating database failure

            var result = await _controller.UpdateUserByID(userId, userDto);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            var response = JObject.FromObject(objectResult.Value);
            Assert.Equal("Failed to update user information.", response["Message"].ToString());
        }


        [Fact]
        public async Task UpdateUserByID_WhenExceptionOccurs_ReturnsStatusCode500()
        {
            var userId = Guid.NewGuid();
            var userDto = new UserDTO
            {
                UserName = "Updated User",
                PhoneNo = "1234567890",
                Address = "Updated Address"
            };

            // Simulate an exception during database access
            _mockUserRepository
                .Setup(repo => repo.GetUserByIDAsync(userId))
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.UpdateUserByID(userId, userDto);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            var response = JObject.FromObject(objectResult.Value);
            Assert.Equal("An error occurred while updating the user information.", response["Message"].ToString());
        }


    }
}
