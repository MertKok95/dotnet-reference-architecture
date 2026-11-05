using DotNetCoreLab.Application.Features.Products.Dtos;
using DotNetCoreLab.Domain.Interfaces;
using MediatR;

namespace DotNetCoreLab.Application.Features.Products.Queries.GetProductById;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IProductRepository _productRepository;

    public GetProductByIdQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);

        if (product is null)
            return null;

        return new ProductDto(product.Id, product.Name, product.Price, product.Stock);
    }
}