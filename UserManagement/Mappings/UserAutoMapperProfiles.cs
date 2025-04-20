using AutoMapper;
using UserManagement.Models;
using UserManagement.Models.DTO;  // Assuming EnumHelper is defined here

namespace UserManagement.Mappings
{
    public class UserAutoMapperProfiles : Profile
    {
        public UserAutoMapperProfiles()
        {
            // Map from User entity to UserDTO.
            CreateMap<User, UserDTO>();

            // Map from UserDTO to User entity.
            CreateMap<UserDTO, User>();

            // Map User entity to UserDTO
            CreateMap<User, UserDTOAuth>();

            // to map DTO back to User
            CreateMap<UserDTOAuth, User>(); 

        }
    }
}

