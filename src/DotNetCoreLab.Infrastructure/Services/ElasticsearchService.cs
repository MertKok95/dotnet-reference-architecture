using System.Net.Http.Json;
using System.Text.Json;
using DotNetCoreLab.Application.Common.Interfaces;
using DotNetCoreLab.Application.Features.Products.Dtos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DotNetCoreLab.Infrastructure.Services;

public class ElasticsearchService : IProductSearchService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ElasticsearchService> _logger;
    private const string IndexName = "products";

    public ElasticsearchService(HttpClient httpClient, IConfiguration configuration, ILogger<ElasticsearchService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        var elasticUrl = configuration["Elasticsearch:Url"] ?? "http://localhost:9200";
        _httpClient.BaseAddress = new Uri(elasticUrl);
    }

    public async Task IndexProductAsync(ProductDto product, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"/{IndexName}/_doc/{product.Id}", product, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "🔍 [ELASTICSEARCH INDEXED] '{ProductName}' arama motoruna indekslendi! (Id: {ProductId})",
                    product.Name, product.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Elasticsearch indeksleme uyarısı: {Message}", ex.Message);
        }
    }

    public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        try
        {
            var searchQuery = new
            {
                query = new
                {
                    match = new
                    {
                        name = new
                        {
                            query = searchTerm,
                            fuzziness = "AUTO"
                        }
                    }
                }
            };

            var response = await _httpClient.PostAsJsonAsync($"/{IndexName}/_search", searchQuery, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return Enumerable.Empty<ProductDto>();

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            var hits = doc.RootElement.GetProperty("hits").GetProperty("hits");

            var results = new List<ProductDto>();
            foreach (var hit in hits.EnumerateArray())
            {
                var source = hit.GetProperty("_source");
                var dto = JsonSerializer.Deserialize<ProductDto>(source.GetRawText(), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (dto != null)
                    results.Add(dto);
            }

            _logger.LogInformation("🔎 [ELASTICSEARCH SEARCH] '{SearchTerm}' araması için {Count} sonuç bulundu.", searchTerm, results.Count);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Elasticsearch arama hatası: {Message}", ex.Message);
            return Enumerable.Empty<ProductDto>();
        }
    }
}