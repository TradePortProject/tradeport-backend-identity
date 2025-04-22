
using System;
using UserManagement.Models.DTO;
using Xunit;

namespace UserManagement.Tests.Models.DTO
{
    public class UserDTOTests
    {
        [Fact]
        public void Can_Create_UserDTO_With_Valid_Data()
        {
            // Arrange
            var dto = new UserDTO
            {
                UserName = "Sowjanya",
                PhoneNo = "9876543210",
                Address = "Singapore"
            };

            // Assert
            Assert.Equal("Sowjanya", dto.UserName);
            Assert.Equal("9876543210", dto.PhoneNo);
            Assert.Equal("Singapore", dto.Address);
        }

        [Fact]
        public void Can_Assign_Empty_Or_Null_Values_To_UserDTO()
        {
            // Arrange
            var dto = new UserDTO
            {
                UserName = null,
                PhoneNo = "",
                Address = null
            };

            // Assert
            Assert.Null(dto.UserName);
            Assert.Equal(string.Empty, dto.PhoneNo);
            Assert.Null(dto.Address);
        }
    }
}
