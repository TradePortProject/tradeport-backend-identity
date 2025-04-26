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

           
            // UserDTO → User (ignore unmapped properties)
            CreateMap<UserDTO, User>()
                .ForMember(dest => dest.UserID, opt => opt.Ignore())
                .ForMember(dest => dest.loginID, opt => opt.Ignore())
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.StrPassword, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.Remarks, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());

            // Map User entity to UserDTO
            CreateMap<User, UserDTOAuth>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.loginID));

            // to map DTO back to User
            CreateMap<UserDTOAuth, User>()
                .ForMember(dest => dest.loginID, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.StrPassword, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNo, opt => opt.Ignore())
                .ForMember(dest => dest.Address, opt => opt.Ignore())
                .ForMember(dest => dest.Remarks, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore());

        }
    }
}

