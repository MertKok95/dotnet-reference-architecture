using Serilog.Core;
using Serilog.Events;

namespace DotNetCoreLab.Api.Handlers;

public class ElasticsearchLogSink : ILogEventSink
{
    private readonly string _elasticUrl;
    private static readonly HttpClient _httpClient = new();

    public ElasticsearchLogSink(string elasticUrl)
    {
        _elasticUrl = elasticUrl.TrimEnd('/');
    }

    public void Emit(LogEvent logEvent)
    {
        var logDocument = new
        {
            timestamp = logEvent.Timestamp.UtcDateTime,
            level = logEvent.Level.ToString(),
            message = logEvent.RenderMessage(),
            exception = logEvent.Exception?.ToString()
        };

        _ = Task.Run(async () =>
        {
            try
            {
                await _httpClient.PostAsJsonAsync($"{_elasticUrl}/system-logs/_doc", logDocument);
            }
            catch
            {
                // Elasticsearch kapalıysa ana uygulamayı etkilemeden devam eder.
            }
        });
    }
}