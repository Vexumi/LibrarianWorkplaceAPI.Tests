using LibrarianWorkplaceAPI.Controllers;
using LibrarianWorkplaceAPI.Interfaces;
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
    public class ReadersControllerTests
    {
        private List<ReaderModel> _readers = new() 
        { 
            new ReaderModel { Id = 1, FullName = "Test 1", DateOfBirth = Convert.ToDateTime("01/01/2000") },
            new ReaderModel { Id = 2, FullName = "Test 2", DateOfBirth = Convert.ToDateTime("02/01/2000") },
            new ReaderModel { Id = 3, FullName = "Test 3", DateOfBirth = Convert.ToDateTime("03/01/2000") },
            new ReaderModel { Id = 4, FullName = "Test 4", DateOfBirth = Convert.ToDateTime("04/01/2000") },
            new ReaderModel { Id = 5, FullName = "Test 5", DateOfBirth = Convert.ToDateTime("05/01/2000") },
        };
        private ReaderModel _readerModel = new () 
        { 
            Id = 1,
            FullName = "Test 1",
            DateOfBirth = Convert.ToDateTime("01/01/2000")
        };
        private ReaderGetModel _readerGetModel = new()
        {
            FullName = "Test 1",
            DateOfBirth = Convert.ToDateTime("01/01/2000")
        };

        private Mock<ILogger<ReadersController>> mockLogging;
        private Mock<ILibraryDbUnit> mockRepo;
        private ReadersController controller;

        public ReadersControllerTests()
        {
            mockLogging = new Mock<ILogger<ReadersController>>();
            mockRepo = new Mock<ILibraryDbUnit>();
            controller = new ReadersController(mockLogging.Object, mockRepo.Object);
        }

        [Fact]
        public async Task GetAllReaders()
        {
            // Arrange
            mockRepo
                .Setup(r => r.Readers.GetAll())
                .ReturnsAsync(_readers);

            // Act
            var actionResult = await controller.GetAllReaders();

            // Assert
            var result = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(_readers, result.Value);
        }

        [Fact]
        public async Task GetReaderById_ReturnsOk()
        {
            // Arrange
            mockRepo.Setup(r => r.Readers.GetById(It.IsAny<int>())).ReturnsAsync(_readerModel);

            // Act
            var actionResult = await controller.GetReaderById(_readerModel.Id);

            // Assert
            var result = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(_readerModel, result.Value);
        }

        [Fact]
        public async Task GetReaderById_ReturnsNotFound()
        {
            // Arrange
            mockRepo.Setup(r => r.Readers.GetById(It.IsAny<int>())).ReturnsAsync((ReaderModel)null);

            // Act
            var actionResult = await controller.GetReaderById(-1);

            // Assert
            var result = Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async Task GetReadersByName_ReturnsOk()
        {
            // Arrange
            mockRepo.Setup(r => r.Readers.GetReaderByName(It.IsAny<string>())).ReturnsAsync(_readers);

            // Act
            var actionResult = await controller.GetReaderByName("Test");

            // Assert
            var result = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(_readers, result.Value);
        }

        [Fact]
        public async Task GetReadersByName_ReturnsNotFound()
        {
            // Arrange
            mockRepo.Setup(r => r.Readers.GetReaderByName(It.IsAny<string>())).ReturnsAsync(new List<ReaderModel>());

            // Act
            var actionResult = await controller.GetReaderByName(_readerModel.FullName);

            // Assert
            var result = Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async Task AddReader_ReturnsOK()
        {
            // Arrange
            mockRepo.Setup(r => r.Readers.Add(It.IsAny<ReaderModel>()));

            // Act
            var actionResult = await controller.AddReader(_readerGetModel);

            // Assert
            var result = Assert.IsType<CreatedAtActionResult>(actionResult);
            mockRepo.Verify(r => r.Readers.Add(It.IsAny<ReaderModel>()), Times.Once());
        }

        [Fact]
        public async Task AddReader_ReturnsBadRequest_ModelStateError()
        {
            // Arrange
            mockRepo.Setup(r => r.Readers.Add(It.IsAny<ReaderModel>()));
            controller.ModelState.AddModelError("ReaderModelError", "Invalid ReaderModel");

            // Act
            var actionResult = await controller.AddReader(_readerGetModel);

            // Assert
            var result = Assert.IsType<BadRequestResult>(actionResult);
            mockRepo.Verify(r => r.Readers.Add(It.IsAny<ReaderModel>()), Times.Never());
        }

        [Fact]
        public async Task DeleteReader_ReturnsOk()
        {
            // Arrange
            mockRepo.Setup(r => r.Readers.Remove(_readerModel));
            mockRepo.Setup(r => r.Readers.GetById(_readerModel.Id)).ReturnsAsync(_readerModel);

            // Act
            var actionResult = await controller.DeleteReader(_readerModel.Id);

            // Assert
            Assert.IsType<NoContentResult>(actionResult);
            mockRepo.Verify(r => r.Readers.Remove(It.IsAny<ReaderModel>()), Times.Once());
        }

        [Fact]
        public async Task DeleteReader_ReturnsNotFound()
        {
            // Arrange
            mockRepo.Setup(r => r.Readers.Remove(It.IsAny<ReaderModel>()));

            // Act
            var actionResult = await controller.DeleteReader(-1);

            // Assert
            Assert.IsType<NotFoundResult>(actionResult);
            mockRepo.Verify(r => r.Readers.Remove(It.IsAny<ReaderModel>()), Times.Never());
        }

        [Fact]
        public async Task ChangeReader_ReturnsBadRequest_ModelStateError()
        {
            // Arrange
            mockRepo.Setup(r => r.Readers.Change(It.IsAny<ReaderModel>()));
            mockRepo.Setup(r => r.Readers.GetById(1)).ReturnsAsync(_readerModel);
            controller.ModelState.AddModelError("ReaderPatchModelError", "Invalid ReaderPatchModel");

            var patchedReader = new ReaderPatchModel { FullName = "Patched Name" };

            // Act
            var actionResult = await controller.ChangeReader(1, patchedReader);

            // Assert
            var result = Assert.IsType<BadRequestResult>(actionResult);
            mockRepo.Verify(r => r.Readers.Change(It.IsAny<ReaderModel>()), Times.Never());
        }

        [Fact]
        public async Task ChangeBook_ReturnsNotFound()
        {
            // Arrange
            mockRepo.Setup(r => r.Readers.Change(It.IsAny<ReaderModel>()));
            mockRepo.Setup(r => r.Readers.GetById(1)).ReturnsAsync((ReaderModel)null);

            var patchedReader = new ReaderPatchModel { FullName = "Patched Name" };

            // Act
            var actionResult = await controller.ChangeReader(1, patchedReader);

            // Assert
            var result = Assert.IsType<NotFoundResult>(actionResult);
            mockRepo.Verify(r => r.Readers.Change(It.IsAny<ReaderModel>()), Times.Never());
        }

        [Fact]
        public async Task ChangeBook_ReturnsOk()
        {
            // Arrange
            mockRepo.Setup(r => r.Readers.Change(It.IsAny<ReaderModel>()));
            mockRepo.Setup(r => r.Readers.GetById(1)).ReturnsAsync(_readerModel);

            var patchedReader = new ReaderPatchModel { FullName = "Patched Name" }; ;

            // Act
            var actionResult = await controller.ChangeReader(1, patchedReader);

            // Assert
            var result = Assert.IsType<NoContentResult>(actionResult);
            mockRepo.Verify(r => r.Readers.Change(It.IsAny<ReaderModel>()), Times.Once());
        }
    }
}
