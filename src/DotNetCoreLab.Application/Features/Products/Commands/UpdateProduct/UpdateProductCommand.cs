using MediatR;

namespace DotNetCoreLab.Application.Features.Products.Commands.UpdateProduct;

public record UpdateProductCommand(Guid Id, string Name, decimal Price, int Stock) : IRequest<bool>;

