using Google.Apis.Auth;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserManagement.Models;

namespace UserManagement.Services
{

    public interface IJwtService
    {
        string GenerateToken(UserCredentials userCredentials);

        Task<GoogleUser?> ValidateToken(string token);

        //void ValidateBearerToken(string token);
    }

    public class JwtService(IConfiguration configuration, ILogger<JwtService> logger) : IJwtService
    {
        private readonly string _jwtKey = configuration["Jwt:Key"] ?? "";
        private readonly string _jwtIssuer = configuration["Jwt:Issuer"] ?? "";
        private readonly string _jwtAudience = configuration["Jwt:Audience"] ?? "";
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<JwtService> _logger = logger;

        public string GenerateToken(UserCredentials userCredentials)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userCredentials.UserID.ToString()),
                new Claim(ClaimTypes.Name, userCredentials.Name),
                new Claim(ClaimTypes.Email, userCredentials.Email),
                new Claim(ClaimTypes.Role, $"{userCredentials.Role}")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtIssuer,
                audience: _jwtAudience,
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        //Validate Google Token
        public async Task<GoogleUser?> ValidateToken(string token)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [_configuration["Google:ClientId"]]
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(token, settings);

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
                _logger.LogError($"Unexpected error during Google token validation: {ex.Message}");

                return null;
            }
        }

        //public void ValidateBearerToken(string token)
        //{
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var key = Encoding.ASCII.GetBytes(_jwtKey);

        //    var validationParameters = new TokenValidationParameters
        //    {
        //        ValidateIssuerSigningKey = true,
        //        IssuerSigningKey = new SymmetricSecurityKey(key),

        //        ValidateIssuer = true,
        //        ValidIssuer = "http://localhost:11145/",

        //        ValidateAudience = true,
        //        ValidAudience = "http://localhost:3001/",

        //        ValidateLifetime = true,
        //        ClockSkew = TimeSpan.Zero

        //    };

        //    try
        //    {
        //        ClaimsPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
        //        _logger.LogInformation("Token is valid.");

        //        foreach (var claim in principal.Claims)
        //        {
        //            _logger.LogInformation($"{claim.Type} : {claim.Value}");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Token validation failed: {ex.Message}");
        //    }
        //}
    }
}
