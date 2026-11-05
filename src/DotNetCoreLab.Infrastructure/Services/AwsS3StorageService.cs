using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using DotNetCoreLab.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DotNetCoreLab.Infrastructure.Services;

public class AwsS3StorageService : IFileStorageService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AwsS3StorageService> _logger;

    public AwsS3StorageService(IConfiguration configuration, ILogger<AwsS3StorageService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var accessKey = _configuration["AWS:AccessKey"] ?? "test";
        var secretKey = _configuration["AWS:SecretKey"] ?? "test";
        var serviceUrl = _configuration["AWS:ServiceUrl"] ?? "http://localhost:4566";
        var bucketName = _configuration["AWS:BucketName"] ?? "dotnetlab-products";

        var credentials = new BasicAWSCredentials(accessKey, secretKey);
        var s3Config = new AmazonS3Config
        {
            ServiceURL = serviceUrl,
            ForcePathStyle = true
        };

        using var s3Client = new AmazonS3Client(credentials, s3Config);

        var bucketsResponse = await s3Client.ListBucketsAsync(cancellationToken);
        if (!bucketsResponse.Buckets.Any(b => b.BucketName == bucketName))
        {
            await s3Client.PutBucketAsync(new PutBucketRequest { BucketName = bucketName }, cancellationToken);
            _logger.LogInformation("☁️ [AWS S3] '{BucketName}' adında yeni bulut kovası oluşturuldu!", bucketName);
        }

        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var putRequest = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = uniqueFileName,
            InputStream = fileStream,
            ContentType = contentType
        };

        await s3Client.PutObjectAsync(putRequest, cancellationToken);

        var fileUrl = $"{serviceUrl}/{bucketName}/{uniqueFileName}";
        _logger.LogInformation("☁️ [AWS S3 UPLOADED] Dosya buluta yüklendi: {FileUrl}", fileUrl);

        return fileUrl;
    }
}