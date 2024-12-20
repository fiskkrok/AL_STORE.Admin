using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Admin.UnitTests.ProductVariants
{
    public class UpdateProductVariantCommandTests
    {
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICurrentUser> _currentUserMock;
        private readonly UpdateProductVariantCommandHandler _handler;

        public UpdateProductVariantCommandTests()
        {
            _productRepositoryMock = new Mock<IProductRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _currentUserMock = new Mock<ICurrentUser>();
            _handler = new UpdateProductVariantCommandHandler(
                _productRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _currentUserMock.Object);
        }

        [Fact]
        public async Task Handle_ProductNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new UpdateProductVariantCommand { ProductId = Guid.NewGuid(), VariantId = Guid.NewGuid() };
            _productRepositoryMock.Setup(repo => repo.GetByIdAsync(command.ProductId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Product.NotFound", result.Error.Code);
        }

        [Fact]
        public async Task Handle_VariantNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new UpdateProductVariantCommand { ProductId = Guid.NewGuid(), VariantId = Guid.NewGuid() };
            var product = new Product("Test", "Test", 100, "USD", "SKU", 10, command.ProductId);

            _productRepositoryMock.Setup(repo => repo.GetByIdAsync(command.ProductId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Variant.NotFound", result.Error.Code);
        }

        [Fact]
        public async Task Handle_ValidCommand_UpdatesVariant()
        {
            // Arrange
            var command = new UpdateProductVariantCommand
            {
                ProductId = Guid.NewGuid(),
                VariantId = Guid.NewGuid(),
                Sku = "Updated SKU",
                Price = 200,
                Currency = "USD",
                Stock = 20
            };
            var product = new Product("Test", "Test", 100, "USD", "SKU", 10, command.ProductId);
            var variant = new ProductVariant("SKU", 100, "USD", 10, command.ProductId);

            product.AddVariant(variant);

            _productRepositoryMock.Setup(repo => repo.GetByIdAsync(command.ProductId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
