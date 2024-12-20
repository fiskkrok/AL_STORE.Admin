using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Admin.UnitTests.Products
{
    public class UpdateProductCommandTests
    {
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<IFileStorage> _fileStorageMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICurrentUser> _currentUserMock;
        private readonly UpdateProductCommandHandler _handler;

        public UpdateProductCommandTests()
        {
            _productRepositoryMock = new Mock<IProductRepository>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _fileStorageMock = new Mock<IFileStorage>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _currentUserMock = new Mock<ICurrentUser>();
            _handler = new UpdateProductCommandHandler(
                _productRepositoryMock.Object,
                _categoryRepositoryMock.Object,
                _fileStorageMock.Object,
                _unitOfWorkMock.Object,
                _currentUserMock.Object);
        }

        [Fact]
        public async Task Handle_ProductNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new UpdateProductCommand { Id = Guid.NewGuid() };
            _productRepositoryMock.Setup(repo => repo.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Product.NotFound", result.Error.Code);
        }

        [Fact]
        public async Task Handle_CategoryNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new UpdateProductCommand { Id = Guid.NewGuid(), CategoryId = Guid.NewGuid() };
            _productRepositoryMock.Setup(repo => repo.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Product("Test", "Test", 100, "USD", "SKU", 10, command.CategoryId));
            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(command.CategoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Category.NotFound", result.Error.Code);
        }

        [Fact]
        public async Task Handle_ValidCommand_UpdatesProduct()
        {
            // Arrange
            var command = new UpdateProductCommand
            {
                Id = Guid.NewGuid(),
                Name = "Updated Name",
                Description = "Updated Description",
                Price = 200,
                Currency = "USD",
                CategoryId = Guid.NewGuid()
            };
            var product = new Product("Test", "Test", 100, "USD", "SKU", 10, command.CategoryId);
            var category = new Category("Category", "Description");

            _productRepositoryMock.Setup(repo => repo.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);
            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(command.CategoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _productRepositoryMock.Verify(repo => repo.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
