using DotNetCoreLab.Application.Features.Products.Dtos;
using MediatR;

namespace DotNetCoreLab.Application.Features.Products.Queries.SearchProducts;

public record SearchProductsQuery(string SearchTerm) : IRequest<IEnumerable<ProductDto>>;