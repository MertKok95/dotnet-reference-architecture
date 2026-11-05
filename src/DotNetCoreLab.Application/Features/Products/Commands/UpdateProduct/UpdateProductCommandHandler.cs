using DotNetCoreLab.Application.Features.Products.Queries.GetAllProducts;
using DotNetCoreLab.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace DotNetCoreLab.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, bool>
{
    private readonly IProductRepository _productRepository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<UpdateProductCommandHandler> _logger;

    public UpdateProductCommandHandler(
        IProductRepository productRepository,
        IDistributedCache cache,
        ILogger<UpdateProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var existingProduct = await _productRepository.GetByIdAsync(request.Id, cancellationToken);

        if (existingProduct is null)
            return false;

        existingProduct.Name = request.Name;
        existingProduct.Price = request.Price;
        existingProduct.Stock = request.Stock;

        await _productRepository.UpdateAsync(existingProduct, cancellationToken);

        await _cache.RemoveAsync(GetAllProductsQueryHandler.CacheKey, cancellationToken);
        _logger.LogInformation("🧹 [REDIS CACHE INVALIDATED] Ürün güncellendi, Redis önbelleği temizlendi.");

        return true;
    }
}