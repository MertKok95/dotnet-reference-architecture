using System.Text.Json;
using DotNetCoreLab.Application.Features.Products.Dtos;
using DotNetCoreLab.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace DotNetCoreLab.Application.Features.Products.Queries.GetAllProducts;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, IEnumerable<ProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<GetAllProductsQueryHandler> _logger;
    public const string CacheKey = "products_all";

    public GetAllProductsQueryHandler(
        IProductRepository productRepository,
        IDistributedCache cache,
        ILogger<GetAllProductsQueryHandler> logger)
    {
        _productRepository = productRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var cachedData = await _cache.GetStringAsync(CacheKey, cancellationToken);

        if (!string.IsNullOrEmpty(cachedData))
        {
            _logger.LogInformation("⚡ [REDIS CACHE HIT] Ürünler veritabanına gidilmeden doğrudan Redis RAM'den getirildi!");
            return JsonSerializer.Deserialize<IEnumerable<ProductDto>>(cachedData)!;
        }

        _logger.LogInformation("🐢 [REDIS CACHE MISS] Redis'te bulunamadı, veritabanından çekilip Redis'e yazılıyor...");
        var products = await _productRepository.GetAllAsync();
        var productDtos = products.Select(p => new ProductDto(p.Id, p.Name, p.Price, p.Stock)).ToList();

        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        };

        var serializedData = JsonSerializer.Serialize(productDtos);
        await _cache.SetStringAsync(CacheKey, serializedData, cacheOptions, cancellationToken);

        return productDtos;
    }
}