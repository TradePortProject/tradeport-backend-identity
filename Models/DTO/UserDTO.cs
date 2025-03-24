using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace UserManagement.Models.DTO
{
    public class UserDTO
    {

        public string UserName { get; set; }

        public string PhoneNo { get; set; }

        public string Address { get; set; }


    }
}
