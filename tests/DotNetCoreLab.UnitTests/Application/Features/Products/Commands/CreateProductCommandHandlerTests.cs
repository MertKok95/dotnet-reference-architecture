using DotNetCoreLab.Application.Common.Interfaces;
using DotNetCoreLab.Application.Features.Products.Commands.CreateProduct;
using DotNetCoreLab.Application.Features.Products.Events;
using DotNetCoreLab.Domain.Entities;
using DotNetCoreLab.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotNetCoreLab.UnitTests.Application.Features.Products.Commands;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<IMessagePublisher> _messagePublisherMock;
    private readonly Mock<ILogger<CreateProductCommandHandler>> _loggerMock;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _cacheMock = new Mock<IDistributedCache>();
        _messagePublisherMock = new Mock<IMessagePublisher>();
        _loggerMock = new Mock<ILogger<CreateProductCommandHandler>>();

        _handler = new CreateProductCommandHandler(
            _productRepositoryMock.Object,
            _cacheMock.Object,
            _messagePublisherMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddProductAndPublishMessage()
    {
        // Test için örnek bir ürün ekleme komutu oluşturulur
        var command = new CreateProductCommand("Test Oyuncu Laptopu", 45000m, 15);

        var resultId = await _handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, resultId);


        // Veritabanı (Repository) AddAsync metodu çağrıldı mı ?
        _productRepositoryMock.Verify(
            repo => repo.AddAsync(
                It.Is<Product>(p => p.Name == "Test Oyuncu Laptopu" && p.Price == 45000m && p.Stock == 15),
                It.IsAny<CancellationToken>()),
            Times.Once);

        // Redis önbelleğini temizleme (RemoveAsync) metodu çağrıldı mı ?
        _cacheMock.Verify(
            cache => cache.RemoveAsync("products_all", It.IsAny<CancellationToken>()),
            Times.Once);


        // Mesaj kuyruğuna (RabbitMQ / AWS SQS) mesaj gönderildi mi ?
        _messagePublisherMock.Verify(
            pub => pub.PublishAsync(
                "product_created_queue",
                It.Is<ProductCreatedEvent>(e => e.Name == "Test Oyuncu Laptopu"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}