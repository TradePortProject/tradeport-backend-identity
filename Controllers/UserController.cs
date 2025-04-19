using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;
using UserManagement.Models;
using UserManagement.Repositories;
using UserManagement.Data;
using Google.Apis.Auth;
using UserManagement.Models.DTO;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using UserManagement.Services;

namespace UserManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _dbcontext;
        private readonly IMapper _mapper;
        private readonly IJwtService _jwtService;

        public UserController(
            IJwtService jwtService,
            IUserRepository userRepository,
            IConfiguration configuration,
            AppDbContext context,
            IMapper mapper)
        {
            _jwtService = jwtService;
            _userRepository = userRepository;
            _configuration = configuration;
            _dbcontext = context;
            _mapper = mapper;
        }

        // ✅ Validate Google Token & Generate JWT
        [HttpPost("validategoogleuser")]
        public async Task<IActionResult> ValidateGoogleUser([FromBody] GoogleAuthRequest request)
        {
            if (string.IsNullOrEmpty(request?.Token))
            {
                return BadRequest("Google token is missing.");
            }

            var googleUser = await ValidateGoogleToken(request.Token);
            if (googleUser == null)
            {
                return Unauthorized("Invalid Google token.");
            }

            var user = await _userRepository.GetUserByEmailAsync(googleUser.Email);
            if (user == null)
            {
                return NotFound(new { message = "User does not exist in the database." });
            }

            var userDto = _mapper.Map<UserDTOAuth>(user);

            var userCredentials = new UserCredentials
            {
                UserID = user.UserID,
                Name = user.UserName,
                Email = user.loginID,
                Role = user.Role
            };

            var jwtToken = GenerateJwtToken(userCredentials);

            return Ok(new
            {
                user = userDto,
                token = jwtToken
            });
        }

        // ✅ Register New User
        [HttpPost("registeruser")]
        public async Task<IActionResult> RegisterUser([FromBody] User newUser)
        {
            if (newUser == null || string.IsNullOrEmpty(newUser.loginID))
            {
                return BadRequest("Invalid user data.");
            }

            if (!string.IsNullOrEmpty(newUser.StrPassword))
            {
                try
                {
                    newUser.Password = Convert.FromBase64String(newUser.StrPassword);
                }
                catch (FormatException)
                {
                    return BadRequest("Invalid Base64 format for password.");
                }
            }
            else
            {
                return BadRequest("Password is required.");
            }

            var existingUser = await _userRepository.GetUserByEmailAsync(newUser.loginID);
            if (existingUser != null)
            {
                return BadRequest("User already exists.");
            }

            newUser.UserID = Guid.NewGuid();
            newUser.IsActive = true;
            newUser.CreatedOn = DateTime.Now;

            var result = await _userRepository.CreateUserAsync(newUser);
            return result ? Ok("User registered successfully.") : StatusCode(500, "Failed to create user.");
        }

        // ✅ Update User Info
        [Authorize]
        [HttpPut("{userID}")]
        public async Task<IActionResult> UpdateUserByID(Guid userID, [FromBody] UserDTO userDTO)
        {
            try
            {
                var user = await _userRepository.GetUserByIDAsync(userID);
                if (user == null)
                {
                    return NotFound(new { Message = "User not found.", ErrorMessage = "Invalid User ID." });
                }

                _mapper.Map(userDTO, user);

                var result = await _userRepository.UpdateUserByIDAsync(userID, user);
                if (!result)
                {
                    return StatusCode(500, new
                    {
                        Message = "Failed to update user information.",
                        ErrorMessage = "Internal server error."
                    });
                }

                return Ok(new
                {
                    Message = "User information updated successfully.",
                    ErrorMessage = string.Empty
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An error occurred while updating the user information.",
                    ErrorMessage = ex.Message
                });
            }
        }

        // ✅ Google Token Validation
        private async Task<GoogleUser> ValidateGoogleToken(string idToken)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _configuration["Google:ClientId"] }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

                return new GoogleUser
                {
                    Sub = payload.Subject,
                    Email = payload.Email,
                    EmailVerified = payload.EmailVerified,
                    Name = payload.Name,
                    Picture = payload.Picture
                };
            }
            catch
            {
                return null;
            }
        }

        // ✅ JWT Generation
        private string GenerateJwtToken(UserCredentials userCredentials)
        {
            var token = _jwtService.GenerateToken(userCredentials);
            _jwtService.ValidateBearerToken(token);
            return token;
        }
    }
}
