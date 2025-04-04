
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

        public UserController(IUserRepository userRepository, IConfiguration configuration, AppDbContext context, IMapper mapper)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _dbcontext = context;
            _mapper = mapper;
        }


        //Validating the incoming Google Token
        [HttpPost("validategoogleuser")]
        public async Task<IActionResult> ValidateGoogleUser([FromBody] string idToken)
        {
            var googleUser = await ValidateGoogleToken(idToken);

            if (googleUser == null)
            {
                return Unauthorized("Invalid Google token.");
            }

            var user = await _userRepository.GetUserByEmailAsync(googleUser.Email);

            if (user != null)
            {
                var userDto = _mapper.Map<UserDTOAuth>(user);

                //// User exists, generate JWT token
                var jwtToken = GenerateJwtToken(googleUser.Email, user.UserName, user.Role);
                return Ok(new { User = userDto, Token = jwtToken });
            }
            else
            {
                return NotFound("User does not exist in the database.");
            }
        }


        //Registering a new user and saving the user details in the DB
        //[Authorize(Roles = "Admin,Manufacturer")]
        // [Authorize]
        [HttpPost("registeruser")]
        public async Task<IActionResult> RegisterUser([FromBody] User newUser)
        {
            if (newUser == null || string.IsNullOrEmpty(newUser.loginID))
            {
                return BadRequest("Invalid user data.");
            }

            // Convert Base64 password
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



        private string GenerateJwtToken(string email, string name, int role)
        {
            string roleString = role.ToString();

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, roleString),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [Authorize]
        [HttpPut]
        [Route("{userID}")]
        public async Task<IActionResult> UpdateUserByID(Guid userID, [FromBody] UserDTO userDTO)
        {
            IActionResult response = null;
            try
            {
                // Check if the product exists
                var user = await _userRepository.GetUserByIDAsync(userID);
                if (user == null)
                {
                    response = NotFound(new
                    {
                        Message = "User not found.",
                        ErrorMessage = "Invalid User ID."
                    });
                }

                // Use AutoMapper to update the existing product with values from the DTO.
                _mapper.Map(userDTO, user);


                // Update the product in the repository
                var result = await _userRepository.UpdateUserByIDAsync(userID, user);

                if (!result)
                {
                    response = StatusCode(500, new
                    {
                        Message = "Failed to update user information.",
                        ErrorMessage = "Internal server error."
                    });
                }

                response = Ok(new
                {
                    Message = "User information updated successfully.",
                    ErrorMessage = string.Empty
                });
            }
            catch (Exception ex)
            {
                response = StatusCode(500, new
                {
                    Message = "An error occurred while updating the user information.",
                    ErrorMessage = ex.Message
                });
            }
            return response;
        }

    }


    // Google User Model
    public class GoogleUser
    {
        public string Sub { get; set; } // Unique Google ID
        public string Email { get; set; }
        public bool EmailVerified { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; } // Profile Picture URL
    }
}
