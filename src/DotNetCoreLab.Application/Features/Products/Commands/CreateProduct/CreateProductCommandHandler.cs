using DotNetCoreLab.Application.Common.Interfaces;
using DotNetCoreLab.Application.Features.Products.Events;
using DotNetCoreLab.Application.Features.Products.Queries.GetAllProducts;
using DotNetCoreLab.Domain.Entities;
using DotNetCoreLab.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace DotNetCoreLab.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _productRepository;
    private readonly IDistributedCache _cache;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        IDistributedCache cache,
        IMessagePublisher messagePublisher,
        ILogger<CreateProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _cache = cache;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Price = request.Price,
            Stock = request.Stock
        };

        // 1. Ürünü Veritabanına Kaydedilir.
        await _productRepository.AddAsync(product, cancellationToken);

        // 2. CACHE INVALIDATION: Yeni ürün eklendiği için Redis'teki eski listeyi silinir.
        await _cache.RemoveAsync(GetAllProductsQueryHandler.CacheKey, cancellationToken);
        
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\n---> 🧹 [REDIS CACHE INVALIDATED] Yeni ürün eklendi, Redis önbelleği temizlendi.\n");
        Console.ResetColor();

        // 3. RABBITMQ PRODUCER: Kuyruğa "Yeni Ürün Eklendi" mesajı bırakılır.
        var productCreatedEvent = new ProductCreatedEvent(
            product.Id,
            product.Name,
            product.Price,
            product.Stock,
            DateTime.UtcNow
        );

        await _messagePublisher.PublishAsync("product_created_queue", productCreatedEvent, cancellationToken);

        return product.Id;
    }
}