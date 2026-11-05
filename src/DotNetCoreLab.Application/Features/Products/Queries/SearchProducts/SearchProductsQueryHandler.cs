using DotNetCoreLab.Application.Common.Interfaces;
using DotNetCoreLab.Application.Features.Products.Dtos;
using MediatR;

namespace DotNetCoreLab.Application.Features.Products.Queries.SearchProducts;

public class SearchProductsQueryHandler : IRequestHandler<SearchProductsQuery, IEnumerable<ProductDto>>
{
    private readonly IProductSearchService _searchService;

    public SearchProductsQueryHandler(IProductSearchService searchService)
    {
        _searchService = searchService;
    }

    public async Task<IEnumerable<ProductDto>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        return await _searchService.SearchProductsAsync(request.SearchTerm, cancellationToken);
    }
}