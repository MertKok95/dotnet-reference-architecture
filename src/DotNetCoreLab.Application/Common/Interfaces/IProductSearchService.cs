
using DotNetCoreLab.Application.Features.Products.Dtos;

namespace DotNetCoreLab.Application.Common.Interfaces;

public interface IProductSearchService
{
    Task IndexProductAsync(ProductDto product, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm, CancellationToken cancellationToken = default);
}