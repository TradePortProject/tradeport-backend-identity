using System;
using AutoMapper;
using UserManagement.Models;
using UserManagement.Models.DTO;
using UserManagement.Mappings;
using Xunit;

namespace UserManagement.Tests.Mappings
{
    public class UserAutoMapperProfilesTests
    {
        private readonly IMapper _mapper;

        public UserAutoMapperProfilesTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<UserAutoMapperProfiles>();
            });

            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void Mapper_Configuration_IsValid()
        {
            Assert.True(true); // config validated in constructor
        }

        [Fact]
        public void Can_Map_User_To_UserDTO()
        {
            var user = new User
            {
                UserName = "Sowjanya",
                PhoneNo = "123456789",
                Address = "Test Address"
            };

            var dto = _mapper.Map<UserDTO>(user);

            Assert.Equal(user.UserName, dto.UserName);
            Assert.Equal(user.PhoneNo, dto.PhoneNo);
            Assert.Equal(user.Address, dto.Address);
        }

        [Fact]
        public void Can_Map_UserDTO_To_User()
        {
            var dto = new UserDTO
            {
                UserName = "Sowjanya",
                PhoneNo = "123456789",
                Address = "Test Address"
            };

            var user = _mapper.Map<User>(dto);

            Assert.Equal(dto.UserName, user.UserName);
            Assert.Equal(dto.PhoneNo, user.PhoneNo);
            Assert.Equal(dto.Address, user.Address);
        }

        [Fact]
        public void Can_Map_User_To_UserDTOAuth()
        {
            var user = new User
            {
                UserID = Guid.NewGuid(),
                UserName = "Sowjanya",
                loginID = "sow@example.com",
                Role = 1,
                IsActive = true
            };

            var dto = _mapper.Map<UserDTOAuth>(user);

            Assert.Equal(user.UserID, dto.UserID);
            Assert.Equal(user.UserName, dto.UserName);
            Assert.Equal(user.loginID, dto.Email);
            Assert.Equal(user.Role, dto.Role);
            Assert.Equal(user.IsActive, dto.IsActive);
        }

        [Fact]
        public void Can_Map_UserDTOAuth_To_User()
        {
            var dto = new UserDTOAuth
            {
                UserID = Guid.NewGuid(),
                UserName = "Sowjanya",
                Email = "sow@example.com",
                Role = 1,
                IsActive = true
            };

            var user = _mapper.Map<User>(dto);

            Assert.Equal(dto.UserID, user.UserID);
            Assert.Equal(dto.UserName, user.UserName);
            Assert.Equal(dto.Email, user.loginID);
            Assert.Equal(dto.Role, user.Role);
            Assert.Equal(dto.IsActive, user.IsActive);
        }
    }
}
