
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json;
using UserManagement.Models;
using UserManagement.Repositories;
using UserManagement.Data;
using Google.Apis.Auth;
using UserManagement.Models.DTO;
using AutoMapper;
using Azure;
using static Google.Apis.Requests.BatchRequest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Data;
using System.Security.Cryptography;
using UserManagement.Services;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Google.Apis.Auth.OAuth2;
using UserManagement.Logger.interfaces;



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
        private readonly IAppLogger<UserController> _logger;

        public UserController(IJwtService jwtService, IUserRepository userRepository, IConfiguration configuration, AppDbContext context, IMapper mapper
            , IAppLogger<UserController> logger)
        {
            _jwtService = jwtService;
            _userRepository = userRepository;
            _configuration = configuration;
            _dbcontext = context;
            _mapper = mapper;
            _logger = logger;
        }


        //Validating the incoming Google Token
        [HttpPost("validategoogleuser")]
        public async Task<IActionResult> ValidateGoogleUser([FromBody] string idToken)
        {
            _logger.LogInformation("ValidateGoogleUser....");

            var googleUser = await ValidateGoogleToken(idToken);

            if (googleUser == null)
            {
                _logger.LogError("Invalid Google token....");
                return Unauthorized("Invalid Google token.");
            }


            if (googleUser != null)
            {
                _logger.LogInformation("Getting User By Email..." + googleUser.Email);

                var user = await _userRepository.GetUserByEmailAsync(googleUser.Email);

                var userDto = _mapper.Map<UserDTOAuth>(user);

                UserCredentials userCredentials = new UserCredentials();
                userCredentials.UserID = user.UserID;
                userCredentials.Name = user.UserName;
                userCredentials.Email = user.loginID;
                userCredentials.Role = user.Role;

                //// User exists, generate JWT token
                _logger.LogInformation("Generating JWT Token");
                var jwtToken = GenerateJwtToken(userCredentials);
                _logger.LogInformation("Return token" + jwtToken);
                return Ok(new { User = userDto, Token = jwtToken });
            }
            else
            {
                _logger.LogError("User does not exist in the database.");
                return NotFound("User does not exist in the database.");
            }
        }


        //Registering a new user and saving the user details in the DB
        //[Authorize(Roles = "Admin,Manufacturer")]
        // [Authorize]
        [HttpPost("registeruser")]
        public async Task<IActionResult> RegisterUser([FromBody] User newUser)
        {
            _logger.LogInformation("Entering Register User...");
            if (newUser == null || string.IsNullOrEmpty(newUser.loginID))
            {
                _logger.LogError("Invalid user data...");
                return BadRequest("Invalid user data.");
            }

            // Convert Base64 password
            if (!string.IsNullOrEmpty(newUser.StrPassword))
            {
                _logger.LogInformation("Password of new user" + newUser.StrPassword);
                try
                {
                    newUser.Password = Convert.FromBase64String(newUser.StrPassword);
                }
                catch (FormatException)
                {
                    _logger.LogError("Invalid Base64 format for password.");
                    return BadRequest("Invalid Base64 format for password.");
                }
            }
            else
            {
                _logger.LogError("Password is required");
                return BadRequest("Password is required.");
            }
            _logger.LogInformation("Getting User by Email" +newUser.loginID);
            var existingUser = await _userRepository.GetUserByEmailAsync(newUser.loginID);
            if (existingUser != null)
            {
                _logger.LogError("Password is required");
                return BadRequest("User already exists.");
            }

            newUser.UserID = Guid.NewGuid();
            newUser.IsActive = true;
            newUser.CreatedOn = DateTime.Now;

            _logger.LogInformation("Getting User by Email" + newUser.loginID);
            var result = await _userRepository.CreateUserAsync(newUser);
            _logger.LogInformation("User registered successfully");
            return result ? Ok("User registered successfully.") : StatusCode(500, "Failed to create user.");
        }

        private async Task<GoogleUser> ValidateGoogleToken(string idToken)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _configuration["Google:ClientId"] } // Validate against client ID
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
            catch (Exception ex)
            {
                return null; // Invalid token
            }
        }



        private string GenerateJwtToken(UserCredentials userCredentials)
        {
            var check = _jwtService.GenerateToken(userCredentials);

            _jwtService.ValidateBearerToken(check);

            return check;
        }

        [Authorize]
        [HttpPut]
        [Route("{userID}")]
        public async Task<IActionResult> UpdateUserByID(Guid userID, [FromBody] UserDTO userDTO)
        {
            _logger.LogInformation("Entering Update User By ID...");
            IActionResult response = null;
            try
            {
                // Check if the product exists
                _logger.LogInformation("Getting User By ID..."+ userID);
                var user = await _userRepository.GetUserByIDAsync(userID);
                if (user == null)
                {
                    _logger.LogError("User not found.");
                    response = NotFound(new
                    {
                        Message = "User not found.",
                        ErrorMessage = "Invalid User ID."
                    });
                }

                // Use AutoMapper to update the existing product with values from the DTO.
                _mapper.Map(userDTO, user);

                _logger.LogInformation("Updating User by ID.." + userID);
                // Update the product in the repository
                var result = await _userRepository.UpdateUserByIDAsync(userID, user);

                if (!result)
                {
                    _logger.LogError("Failed to update user information");
                    response = StatusCode(500, new
                    {
                        Message = "Failed to update user information.",
                        ErrorMessage = "Internal server error."
                    });
                }

                _logger.LogInformation("User information updated successfully.");
                response = Ok(new
                {
                    Message = "User information updated successfully.",
                    ErrorMessage = string.Empty
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the user information", ex.Message);
                response = StatusCode(500, new
                {
                    Message = "An error occurred while updating the user information.",
                    ErrorMessage = ex.Message
                });
            }
            return response;
        }

    }


}
