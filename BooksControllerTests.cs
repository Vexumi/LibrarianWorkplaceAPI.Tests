using LibrarianWorkplaceAPI.Controllers;
using LibrarianWorkplaceAPI.Core.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection.PortableExecutable;

namespace LibrarianWorkplaceAPI.Tests
{
    public class BooksControllerTests
    {
        private BookGetModel _bookGetModel = new() { Title = "Best Book 1", Author = "NoName", NumberOfCopies = 10, ReleaseDate = Convert.ToDateTime("01/01/2000")};
        private BookModel _bookModel = new () { VendorCode = 1, Title = "Best Book 1", Author = "NoName", NumberOfCopies = 10, ReleaseDate = Convert.ToDateTime("01/01/2000") };
        private List<BookModel> _books = new List<BookModel>() {
                new () { VendorCode = 1, Title = "Best Book 1", Author = "NoName", NumberOfCopies = 10, ReleaseDate = Convert.ToDateTime("01/01/2000") },
                new () { VendorCode = 2, Title = "Best Book 2", Author = "NoName", NumberOfCopies = 10, ReleaseDate = Convert.ToDateTime("01/01/2000") },
                new () { VendorCode = 3, Title = "Best Book 3", Author = "NoName", NumberOfCopies = 10, ReleaseDate = Convert.ToDateTime("01/01/2000") },
            };
        private ReaderModel _readerModel = new ReaderModel()
        {
            Id = 1,
            FullName = "Test",
            DateOfBirth = Convert.ToDateTime("01/01/2000"),
        };

        private Mock<ILogger<BooksController>> mockLogging;
        private Mock<ILibraryDbUnit> mockRepo;
        private BooksController controller;

        public BooksControllerTests()
        {
            mockLogging = new Mock<ILogger<BooksController>>();
            mockRepo = new Mock<ILibraryDbUnit>();
            controller = new BooksController(mockLogging.Object, mockRepo.Object);
        }

        [Fact]
        public async Task GetAllBooks()
        {
            // Arrange
            mockRepo
                .Setup(r => r.Books.GetAll())
                .ReturnsAsync(_books);

            // Act
            var actionResult = await controller.GetAllBooks();

            // Assert
            var result = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(_books, result.Value);
        }

        [Fact]
        public async Task GetBookById_ReturnsOk()
        {
            // Arrange
            mockRepo.Setup(r => r.Books.GetById(It.IsAny<int>())).ReturnsAsync(_bookModel);

            // Act
            var actionResult = await controller.GetBookById(_bookModel.VendorCode);

            // Assert
            var result = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(_bookModel, result.Value);
        }

        [Fact]
        public async Task GetBookById_ReturnsNotFound()
        {
            // Arrange
            mockRepo.Setup(r => r.Books.GetById(It.IsAny<int>())).ReturnsAsync((BookModel)null);

            // Act
            var actionResult = await controller.GetBookById(-1);

            // Assert
            var result = Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async Task GetBookByTitle_ReturnsOk()
        {
            // Arrange
            mockRepo.Setup(r => r.Books.GetByTitle(It.IsAny<string>())).ReturnsAsync(new List<BookModel>() { _bookModel });

            // Act
            var actionResult = await controller.GetBookByTitle(_bookModel.Title);

            // Assert
            var result = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(new List<BookModel>() { _bookModel }, result.Value);
        }

        [Fact]
        public async Task GetBookByTitle_ReturnsNotFound()
        {
            // Arrange
            mockRepo.Setup(r => r.Books.GetByTitle(It.IsAny<string>())).ReturnsAsync((IEnumerable<BookModel>)null);

            // Act
            var actionResult = await controller.GetBookByTitle(_bookModel.Title);

            // Assert
            var result = Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async Task GetAvailableBooks_ReturnsBooks()
        {
            // Arrange
            mockRepo.Setup(r => r.Books.GetAvailableBooks()).ReturnsAsync(_books);

            // Act
            var actionResult = await controller.GetAvailableBooks();

            // Assert
            var result = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(_books, result.Value);
        }

        [Fact]
        public async Task GetAvailableBooks_ReturnsOkAllBooksAreBusy()
        {
            // Arrange
            mockRepo.Setup(r => r.Books.GetAvailableBooks()).ReturnsAsync((IEnumerable<BookModel>)null);

            // Act
            var actionResult = await controller.GetAvailableBooks();

            // Assert
            var result = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal("All books are busy", result.Value);
        }

        [Fact]
        public async Task GetGivedBooks_ReturnsBooks()
        {
            // Arrange
            mockRepo.Setup(r => r.Books.GetGivedBooks()).ReturnsAsync(_books);

            // Act
            var actionResult = await controller.GetGivedBooks();

            // Assert
            var result = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(_books, result.Value);
        }

        [Fact]
        public async Task GetGivedBooks_ReturnsOkAllBooksAreFree()
        {
            // Arrange
            mockRepo.Setup(r => r.Books.GetGivedBooks()).ReturnsAsync((IEnumerable<BookModel>)null);

            // Act
            var actionResult = await controller.GetGivedBooks();

            // Assert
            var result = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal("All books are free", result.Value);
        }

        [Fact]
        public async Task AddBook_ReturnsOK()
        {
            // Arrange
            mockRepo.Setup(r => r.Books.Add(It.IsAny<BookModel>()));

            // Act
            var actionResult = await controller.AddBook(_bookGetModel);

            // Assert
            var result = Assert.IsType<CreatedAtActionResult>(actionResult);
            mockRepo.Verify(r => r.Books.Add(It.IsAny<BookModel>()), Times.Once());
        }
        
        [Fact]
        public async Task AddBook_ReturnsBadRequest_ModelStateError()
        {
            // Arrange
            mockRepo.Setup(r => r.Books.Add(It.IsAny<BookModel>()));
            controller.ModelState.AddModelError("BookModelError", "Invalid BookModel");

            // Act
            var actionResult = await controller.AddBook(_bookGetModel);

            // Assert
            var result = Assert.IsType<BadRequestResult>(actionResult);
            mockRepo.Verify(r => r.Books.Add(It.IsAny<BookModel>()), Times.Never());
        }

        [Fact]
        public async Task DeleteBook_ReturnsOk()
        {
            // Arrange
            mockRepo.Setup(r => r.Books.Remove(_bookModel));
            mockRepo.Setup(r => r.Books.GetById(_bookModel.VendorCode)).ReturnsAsync(_bookModel);

            // Act
            var actionResult = await controller.DeleteBook(_bookModel.VendorCode);

            // Assert
            Assert.IsType<NoContentResult>(actionResult);
            mockRepo.Verify(r => r.Books.Remove(It.IsAny<BookModel>()), Times.Once());
        }

        [Fact]
        public async Task DeleteBook_ReturnsNotFound()
        {
            // Arrange
            mockRepo.Setup(r => r.Books.Remove(It.IsAny<BookModel>()));

            // Act
            var actionResult = await controller.DeleteBook(-1);

            // Assert
            Assert.IsType<NotFoundResult>(actionResult);
            mockRepo.Verify(r => r.Books.Remove(It.IsAny<BookModel>()), Times.Never());
        }

        [Fact]
        public async Task ChangeBook_ReturnsBadRequest_ModelStateError()
        {
            // Arrange
            mockRepo.Setup(r => r.Books.Change(It.IsAny<BookModel>()));
            mockRepo.Setup(r => r.Books.GetById(1)).ReturnsAsync(_bookModel);
            controller.ModelState.AddModelError("BookPatchModelError", "Invalid BookPatchModel");

            var patchedBook = new BookPatchModel { Author = "Test" };

            // Act
            var actionResult = await controller.ChangeBook(1, patchedBook);

            // Assert
            var result = Assert.IsType<BadRequestResult>(actionResult);
            mockRepo.Verify(r => r.Books.Change(It.IsAny<BookModel>()), Times.Never());
        } 

        [Fact]
        public async Task ChangeBook_ReturnsNotFound()
        {
            // Arrange
            mockRepo.Setup(r => r.Books.Change(It.IsAny<BookModel>()));
            mockRepo.Setup(r => r.Books.GetById(1)).ReturnsAsync((BookModel)null);
            var patchedBook = new BookPatchModel { Author = "Test" };

            // Act
            var actionResult = await controller.ChangeBook(1, patchedBook);

            // Assert
            var result = Assert.IsType<NotFoundResult>(actionResult);
            mockRepo.Verify(r => r.Books.Change(It.IsAny<BookModel>()), Times.Never());
        }

        [Fact]
        public async Task ChangeBook_ReturnsOk()
        {
            // Arrange
            mockRepo.Setup(r => r.Books.Change(It.IsAny<BookModel>()));
            mockRepo.Setup(r => r.Books.GetById(1)).ReturnsAsync(_bookModel);

            // Act
            var patchedBook = new BookPatchModel { Author = "Test" };
            var actionResult = await controller.ChangeBook(1, patchedBook);

            // Assert
            var result = Assert.IsType<NoContentResult>(actionResult);
            mockRepo.Verify(r => r.Books.Change(It.IsAny<BookModel>()), Times.Once());
        }

        [Fact]
        public async Task TakeBook_ReturnsNoContent()
        {
            // Arrange
            mockRepo.Setup(r => r.Readers.GetById(It.IsAny<int>())).ReturnsAsync(_readerModel);
            mockRepo.Setup(r => r.Books.GetById(It.IsAny<int>())).ReturnsAsync(_bookModel);

            // Act
            var actionResult = await controller.TakeBook(1, 1);

            // Assert
            Assert.IsType<NoContentResult>(actionResult);
            mockRepo.Verify(r => r.Books.Take(It.IsAny<ReaderModel>(), It.IsAny<BookModel>()), Times.Once());
        }

        [Theory]
        [MemberData(nameof(TakeBook_ErrorTestCases))]
        public async Task TakeBook_Errors(ReaderModel reader, BookModel book, Type responseType, string answer)
        {
            // Arrange
            mockRepo.Setup(r => r.Readers.GetById(It.IsAny<int>())).ReturnsAsync(reader);
            mockRepo.Setup(r => r.Books.GetById(It.IsAny<int>())).ReturnsAsync(book);

            // Act
            var actionResult = await controller.TakeBook(1, 1);

            // Assert
            Assert.IsType(responseType, actionResult);
            Assert.Equal(answer, (actionResult as ObjectResult)?.Value);
            mockRepo.Verify(r => r.Books.Take(It.IsAny<ReaderModel>(), It.IsAny<BookModel>()), Times.Never());
        }

        public static TheoryData<ReaderModel, BookModel, Type, string> TakeBook_ErrorTestCases()
        {
            BookModel _bookModelBusy = new() { 
                VendorCode = 1,
                Title = "Best Book 1",
                Author = "NoName", 
                NumberOfCopies = 1, 
                ReleaseDate = Convert.ToDateTime("01/01/2000"), 
                Readers = new List<int>() { 1 } 
            };
            ReaderModel _ReaderModelAlredyTakenBook = new ReaderModel()
            {
                Id = 1,
                FullName = "Test",
                DateOfBirth = Convert.ToDateTime("01/01/2000"),
                Books = new List<int>() { 1 }
            };
            ReaderModel _readerModel = new ReaderModel()
                {
                    Id = 1,
                    FullName = "Test",
                    DateOfBirth = Convert.ToDateTime("01/01/2000"),
                };
            BookModel _bookModel = new() { 
                VendorCode = 1, 
                Title = "Best Book 1", 
                Author = "NoName", 
                NumberOfCopies = 10,
                ReleaseDate = Convert.ToDateTime("01/01/2000")
            };

            return new TheoryData<ReaderModel, BookModel, Type, string>()
            {
                { _readerModel, (BookModel)null, typeof(NotFoundObjectResult), "Book" },
                { (ReaderModel)null, _bookModel, typeof(NotFoundObjectResult),"Reader" },
                { _readerModel, _bookModelBusy, typeof(BadRequestObjectResult), "All books are busy" },
                { _ReaderModelAlredyTakenBook, _bookModel, typeof(BadRequestObjectResult), "Reader has already taken this book!" }
            };
        }
    }
}