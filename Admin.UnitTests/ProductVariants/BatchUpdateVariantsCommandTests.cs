using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Admin.Application;
namespace Admin.UnitTests.ProductVariants
{
    public class BatchUpdateVariantsCommandTests
    {
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICurrentUser> _currentUserMock;
        private readonly BatchUpdateVariantsCommandHandler _handler;

        public BatchUpdateVariantsCommandTests()
        {
            _productRepositoryMock = new Mock<IProductRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _currentUserMock = new Mock<ICurrentUser>();
            _handler = new BatchUpdateVariantsCommandHandler(
                _productRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _currentUserMock.Object);
        }

        [Fact]
        public async Task Handle_ProductNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new BatchUpdateVariantsCommand
            {
                Variants = new List<UpdateVariantRequest>
                {
                    new UpdateVariantRequest { ProductId = Guid.NewGuid(), VariantId = Guid.NewGuid() }
                }
            };
            _productRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Product", result.Error.Code);
        }

        [Fact]
        public async Task Handle_VariantNotFound_ReturnsFailure()
        {
            // Arrange
            var command = new BatchUpdateVariantsCommand
            {
                Variants = new List<UpdateVariantRequest>
                {
                    new UpdateVariantRequest { ProductId = Guid.NewGuid(), VariantId = Guid.NewGuid() }
                }
            };
            var product = new Product("Test", "Test", 100, "USD", "SKU", 10, command.Variants.First().ProductId);

            _productRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Variant", result.Error.Code);
        }

        [Fact]
        public async Task Handle_ValidCommand_UpdatesVariants()
        {
            // Arrange
            var command = new BatchUpdateVariantsCommand
            {
                Variants = new List<UpdateVariantRequest>
                {
                    new UpdateVariantRequest { ProductId = Guid.NewGuid(), VariantId = Guid.NewGuid(), Sku = "Updated SKU", Price = 200, Stock = 20 }
                }
            };
            var product = new Product("Test", "Test", 100, "USD", "SKU", 10, command.Variants.First().ProductId);
            var variant = new ProductVariant("SKU", 100, "USD", 10, command.Variants.First().ProductId);

            product.AddVariant(variant);

            _productRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
