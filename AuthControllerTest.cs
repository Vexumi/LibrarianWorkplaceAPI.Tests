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
    public class AuthControllerTests
    {
        private Mock<ILogger<AuthController>> mockLogging;
        private Mock<ITokenManager> mockTokenManager;
        private AuthController controller;

        private UserCredential userCredential = new()
        {
            UserName = "TestUserName",
            Password = "TestPassword"
        };

        public AuthControllerTests()
        {
            mockLogging = new();
            mockTokenManager = new();
            controller = new(mockLogging.Object, mockTokenManager.Object);
        }

        [Fact]
        public async Task AuthenticateUser_ReturnsOk()
        {
            // Arrange
            mockTokenManager.Setup(tm => tm.Authenticate(It.IsAny<string>(), It.IsAny<string>())).Returns("123456789abcdef");

            // Act
            var actionResult = await controller.Authenticate(userCredential);

            // Assert
            var result = Assert.IsType<OkObjectResult>(actionResult.Result);
        }

        [Fact]
        public async Task AuthenticateUser_ReturnsUnauthorized()
        {
            // Arrange
            mockTokenManager.Setup(tm => tm.Authenticate(It.IsAny<string>(), It.IsAny<string>())).Returns(String.Empty);

            // Act
            var actionResult = await controller.Authenticate(userCredential);

            // Assert
            var result = Assert.IsType<UnauthorizedResult>(actionResult.Result);
        }
    }
}
