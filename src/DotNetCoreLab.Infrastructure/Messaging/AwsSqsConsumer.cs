using System.Text.Json;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using DotNetCoreLab.Application.Common.Interfaces;
using DotNetCoreLab.Application.Features.Products.Dtos;
using DotNetCoreLab.Application.Features.Products.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNetCoreLab.Infrastructure.Messaging;

public class AwsSqsConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IProductSearchService _searchService;
    private readonly ILogger<AwsSqsConsumer> _logger;
    public const string QueueName = "product_created_queue";

    public AwsSqsConsumer(
        IConfiguration configuration,
        IProductSearchService searchService,
        ILogger<AwsSqsConsumer> logger)
    {
        _configuration = configuration;
        _searchService = searchService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var accessKey = _configuration["AWS:AccessKey"] ?? "test";
        var secretKey = _configuration["AWS:SecretKey"] ?? "test";
        var serviceUrl = _configuration["AWS:ServiceUrl"] ?? "http://localhost:4566";

        var credentials = new BasicAWSCredentials(accessKey, secretKey);
        var sqsConfig = new AmazonSQSConfig { ServiceURL = serviceUrl };

        using var sqsClient = new AmazonSQSClient(credentials, sqsConfig);

        string? queueUrl = null;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (queueUrl is null)
                {
                    var queueResponse = await sqsClient.CreateQueueAsync(QueueName, stoppingToken);
                    queueUrl = queueResponse.QueueUrl;
                }

                var receiveRequest = new ReceiveMessageRequest
                {
                    QueueUrl = queueUrl,
                    MaxNumberOfMessages = 5,
                    WaitTimeSeconds = 5
                };

                var response = await sqsClient.ReceiveMessageAsync(receiveRequest, stoppingToken);

                foreach (var message in response.Messages)
                {
                    var productEvent = JsonSerializer.Deserialize<ProductCreatedEvent>(message.Body);

                    if (productEvent != null)
                    {
                        _logger.LogInformation(
                            "☁️ [AWS SQS CONSUMER] Bulut kuyruğundan mesaj yakalandı: {ProductName} (Id: {ProductId})",
                            productEvent.Name, productEvent.ProductId);

                        var dto = new ProductDto(productEvent.ProductId, productEvent.Name, productEvent.Price, productEvent.Stock);
                        await _searchService.IndexProductAsync(dto, stoppingToken);
                    }

                    await sqsClient.DeleteMessageAsync(queueUrl, message.ReceiptHandle, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("AWS SQS dinleme hatası: {Message}", ex.Message);
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}