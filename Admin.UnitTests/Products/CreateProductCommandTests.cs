using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Admin.UnitTests.Products
{
    public class CreateProductCommandTests
    {
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<IFileStorage> _fileStorageMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICurrentUser> _currentUserMock;
        private readonly CreateProductCommandHandler _handler;

        public CreateProductCommandTests()
        {
            _productRepositoryMock = new Mock<IProductRepository>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _fileStorageMock = new Mock<IFileStorage>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _currentUserMock = new Mock<ICurrentUser>();
            _handler = new CreateProductCommandHandler(
                _productRepositoryMock.Object,
                _categoryRepositoryMock.Object,
                _fileStorageMock.Object,
                _unitOfWorkMock.Object,
                _currentUserMock.Object);
        }

        [Fact]
        public async Task Handle_CategoryNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new CreateProductCommand
            {
                Name = "Test Product",
                Description = "Test Description",
                Price = 100,
                Currency = "USD",
                CategoryId = Guid.NewGuid()
            };
            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(command.CategoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Category.NotFound", result.Error.Code);
        }

        [Fact]
        public async Task Handle_ValidCommand_CreatesProduct()
        {
            // Arrange
            var command = new CreateProductCommand
            {
                Name = "Test Product",
                Description = "Test Description",
                Price = 100,
                Currency = "USD",
                CategoryId = Guid.NewGuid()
            };
            var category = new Category("Category", "Description");

            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(command.CategoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _productRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
