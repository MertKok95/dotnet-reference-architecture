using DotNetCoreLab.Application.Features.Products.Dtos;
using MediatR;

namespace DotNetCoreLab.Application.Features.Products.Queries.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;