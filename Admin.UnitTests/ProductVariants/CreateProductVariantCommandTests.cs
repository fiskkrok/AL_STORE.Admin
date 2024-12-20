using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Admin.UnitTests.ProductVariants
{
    public class CreateProductVariantCommandTests
    {
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICurrentUser> _currentUserMock;
        private readonly CreateProductVariantCommandHandler _handler;

        public CreateProductVariantCommandTests()
        {
            _productRepositoryMock = new Mock<IProductRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _currentUserMock = new Mock<ICurrentUser>();
            _handler = new CreateProductVariantCommandHandler(
                _productRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _currentUserMock.Object);
        }

        [Fact]
        public async Task Handle_ProductNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new CreateProductVariantCommand(Guid.NewGuid(), "SKU", 100, "USD", 10, new List<ProductAttributeRequest>());
            _productRepositoryMock.Setup(repo => repo.GetByIdAsync(command.ProductId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Product.NotFound", result.Error.Code);
        }

        [Fact]
        public async Task Handle_ValidCommand_CreatesVariant()
        {
            // Arrange
            var command = new CreateProductVariantCommand(Guid.NewGuid(), "SKU", 100, "USD", 10, new List<ProductAttributeRequest>());
            var product = new Product("Test", "Test", 100, "USD", "SKU", 10, command.ProductId);

            _productRepositoryMock.Setup(repo => repo.GetByIdAsync(command.ProductId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _productRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<ProductVariant>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

