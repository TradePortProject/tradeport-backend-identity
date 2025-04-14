namespace UserManagement.Models
{
    public class UserCredentials
    {
        public Guid UserID { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public int Role { get; set; }
    }

}
