using LibrarianWorkplaceAPI.Controllers;
using LibrarianWorkplaceAPI.Core.Auth;
using LibrarianWorkplaceAPI.Core.Repositories.Interfaces;
using LibrarianWorkplaceAPI.Models.GetModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarianWorkplaceAPI.Tests
{
    public class RegisterControllerTests
    {
        private Mock<ILogger<RegisterController>> mockLogging;
        private Mock<ITokenManager> mockTokenManager;
        private RegisterController controller;

        private RegisterUserModel registerUserModel = new() { UserName = "TestUserName", 
                                                              Password = "TestPassword",
                                                              LibraryName = "TestLibrary",
                                                              Address = "TestAddress" };

        public RegisterControllerTests()
        {
            mockLogging = new();
            mockTokenManager = new();
            controller = new(mockLogging.Object, mockTokenManager.Object);
        }

        [Fact]
        public async Task RegisterUser_ReturnsCreated()
        {
            // Arrange
            mockTokenManager.Setup( tm => tm.Register(It.IsAny<UserModel>())).Returns(true);

            // Act
            var actionResult = await controller.Registration(registerUserModel);

            // Assert
            var result = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        }

        [Fact]
        public async Task RegisterUser_ReturnsBadRequest()
        {
            // Arrange
            mockTokenManager.Setup(tm => tm.Register(It.IsAny<UserModel>())).Returns((bool?)null);

            // Act
            var actionResult = await controller.Registration(registerUserModel);

            // Assert
            var result = Assert.IsType<BadRequestResult>(actionResult.Result);
        }
    }
}
