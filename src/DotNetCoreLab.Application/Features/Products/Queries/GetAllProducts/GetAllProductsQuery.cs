using DotNetCoreLab.Application.Features.Products.Dtos;
using MediatR;

namespace DotNetCoreLab.Application.Features.Products.Queries.GetAllProducts;

public record GetAllProductsQuery() : IRequest<IEnumerable<ProductDto>>;
