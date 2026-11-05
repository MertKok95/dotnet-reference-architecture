namespace DotNetCoreLab.Application.Features.Products.Events;

// Kuyruğa (RabbitMQ'ya) bırakacağımız mesajın içeriği
public record ProductCreatedEvent(
    Guid ProductId,
    string Name,
    decimal Price,
    int Stock,
    DateTime CreatedAt
);