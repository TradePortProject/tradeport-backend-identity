namespace UserManagement.Models
{
    // Google User Model
    public class GoogleUser
    {
        public UserCredentials? UserCredential { get; set; }
        public string? Sub { get; set; } // Unique Google ID

        //public string Email { get; set; }
        //public string Name { get; set; }
        public string? Picture { get; set; } // Profile Picture URL
        public bool EmailVerified { get; set; }
    }

}
