using System.Text;
using System.Text.Json;
using DotNetCoreLab.Application.Common.Interfaces;
using DotNetCoreLab.Application.Features.Products.Dtos;
using DotNetCoreLab.Application.Features.Products.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DotNetCoreLab.Infrastructure.Services;

public class ProductCreatedConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IProductSearchService _searchService;
    private readonly ILogger<ProductCreatedConsumer> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    public const string QueueName = "product_created_queue";

    public ProductCreatedConsumer(
        IConfiguration configuration,
        IProductSearchService searchService,
        ILogger<ProductCreatedConsumer> logger)
    {
        _configuration = configuration;
        _searchService = searchService;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var hostName = _configuration["RabbitMQ:HostName"] ?? "localhost";
            var factory = new ConnectionFactory { HostName = hostName };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);
                var productEvent = JsonSerializer.Deserialize<ProductCreatedEvent>(messageJson);

                if (productEvent != null)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"\n---> 📬 [RABBITMQ CONSUMER] Kuyruktan mesaj yakalandı: {productEvent.Name}");
                    Console.ResetColor();

                    // Yeni ürün otomatik olarak Elasticsearch'e indekslenir.
                    var dto = new ProductDto(productEvent.ProductId, productEvent.Name, productEvent.Price, productEvent.Stock);
                    await _searchService.IndexProductAsync(dto, stoppingToken);

                    // Abonelere bildirim e-postası gönderilir (simülasyon)
                    await Task.Delay(500, stoppingToken);
                }

                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("RabbitMQ sunucusuna bağlanılamadı: {Message}", ex.Message);
        }

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}