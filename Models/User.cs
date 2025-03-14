
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace UserManagement.Models
{
    public class User
    {
        [Key]
        [SwaggerSchema(ReadOnly = true)] // Prevents UserID from being required in Swagger
        public Guid UserID { get; set; }

        [Required]
        [MaxLength(255)]       
        public string loginID { get; set; }

        [Required]
        [MaxLength(255)]
        public string UserName { get; set; } 

       
        [MaxLength(50)]
        public byte[]? Password { get; set; } 

        [MaxLength(50)]
        public string? StrPassword { get; set; }  
       

        [Required]
        public int Role { get; set; } 

        [MaxLength(20)]
        public string? PhoneNo { get; set; } 

        [MaxLength(500)]
        public string? Address { get; set; } 

        [MaxLength(500)]
        public string? Remarks { get; set; } 

        public DateTime? CreatedOn { get; set; } 

        public bool? IsActive { get; set; }

       // public User NewUser { get; set; }
    }
}
