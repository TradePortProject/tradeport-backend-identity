using Google.Apis.Auth.OAuth2;

namespace UserManagement.Models
{
    public class GoogleUser
    {
        public string? Sub { get; set; } // Unique Google ID

        public string Email { get; set; }
        public string Name { get; set; }
        public string? Picture { get; set; } // Profile Picture URL
        public bool EmailVerified { get; set; }
    }

}