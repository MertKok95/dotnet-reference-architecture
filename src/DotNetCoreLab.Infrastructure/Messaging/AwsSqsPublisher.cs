using System.Text.Json;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using DotNetCoreLab.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DotNetCoreLab.Infrastructure.Messaging;

public class AwsSqsPublisher : IMessagePublisher
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AwsSqsPublisher> _logger;

    public AwsSqsPublisher(IConfiguration configuration, ILogger<AwsSqsPublisher> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task PublishAsync<T>(string queueName, T message, CancellationToken cancellationToken = default)
    {
        try
        {
            var accessKey = _configuration["AWS:AccessKey"] ?? "test";
            var secretKey = _configuration["AWS:SecretKey"] ?? "test";
            var serviceUrl = _configuration["AWS:ServiceUrl"] ?? "http://localhost:4566";

            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            var sqsConfig = new AmazonSQSConfig { ServiceURL = serviceUrl };

            using var sqsClient = new AmazonSQSClient(credentials, sqsConfig);

            var createQueueResponse = await sqsClient.CreateQueueAsync(
                new CreateQueueRequest { QueueName = queueName },
                cancellationToken);

            var queueUrl = createQueueResponse.QueueUrl;
            var json = JsonSerializer.Serialize(message);

            var sendRequest = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = json
            };

            var response = await sqsClient.SendMessageAsync(sendRequest, cancellationToken);

            _logger.LogInformation(
                "☁️ [AWS SQS PRODUCER] '{QueueName}' bulut kuyruğuna mesaj bırakıldı! MessageId: {MessageId} | İçerik: {Payload}",
                queueName, response.MessageId, json);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("AWS SQS mesaj gönderilemedi: {Message}", ex.Message);
        }
    }
}