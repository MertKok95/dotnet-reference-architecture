using DotNetCoreLab.Application.Features.Products.Queries.GetAllProducts;
using DotNetCoreLab.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace DotNetCoreLab.Application.Features.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IProductRepository _productRepository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<DeleteProductCommandHandler> _logger;

    public DeleteProductCommandHandler(
        IProductRepository productRepository,
        IDistributedCache cache,
        ILogger<DeleteProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var existingProduct = await _productRepository.GetByIdAsync(request.Id, cancellationToken);

        if (existingProduct is null)
            return false;

        await _productRepository.DeleteAsync(request.Id, cancellationToken);

        await _cache.RemoveAsync(GetAllProductsQueryHandler.CacheKey, cancellationToken);
        _logger.LogInformation("🧹 [REDIS CACHE INVALIDATED] Ürün silindi, Redis önbelleği temizlendi.");

        return true;
    }
}