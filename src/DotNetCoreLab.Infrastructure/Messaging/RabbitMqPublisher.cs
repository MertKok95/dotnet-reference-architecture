using System.Text;
using System.Text.Json;
using DotNetCoreLab.Application.Common.Interfaces; 
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace DotNetCoreLab.Infrastructure.Messaging;

public class RabbitMqPublisher : IMessagePublisher
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(IConfiguration configuration, ILogger<RabbitMqPublisher> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task PublishAsync<T>(string queueName, T message, CancellationToken cancellationToken = default)
    {
        try
        {
            var hostName = _configuration["RabbitMQ:HostName"] ?? "localhost";

            var factory = new ConnectionFactory { HostName = hostName };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            channel.BasicPublish(
                exchange: string.Empty,
                routingKey: queueName,
                basicProperties: null,
                body: body);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n---> 🐇 [RABBITMQ PRODUCER] '{queueName}' kuyruğuna yeni mesaj bırakıldı: {json}\n");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            _logger.LogWarning("RabbitMQ mesaj gönderilemedi (Docker kapalı olabilir): {Message}", ex.Message);
        }

        return Task.CompletedTask;
    }
}