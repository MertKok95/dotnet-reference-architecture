namespace DotNetCoreLab.Application.Features.Products.Dtos;

public record ProductDto(Guid Id, string Name, decimal Price, int Stock);