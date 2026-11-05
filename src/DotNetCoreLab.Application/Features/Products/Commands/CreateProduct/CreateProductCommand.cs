using MediatR;

namespace DotNetCoreLab.Application.Features.Products.Commands.CreateProduct;

public record CreateProductCommand(string Name, decimal Price, int Stock) : IRequest<Guid>;