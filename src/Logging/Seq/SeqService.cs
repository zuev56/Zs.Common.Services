using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zs.Common.Services.Http;

namespace Zs.Common.Services.Logging.Seq;

public sealed class SeqService : ISeqService
{
    private readonly string _token;
    private readonly ILogger<SeqService>? _logger;

    public string Url { get; } // Example: http://localhost:5341

    public SeqService(string url, string token, ILogger<SeqService>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(token);

        Url = url.TrimEnd('/');

        _token = token;
        _logger = logger;
    }

    public async Task<List<SeqEvent>> GetLastEventsAsync(int take = 10, params int[] signals)
        => await GetLastEventsAsync(DateTime.MinValue, take, signals).ConfigureAwait(false);

    public async Task<List<SeqEvent>> GetLastEventsAsync(DateTime fromDate, int take = 10, params int[] signals)
    {
        if (signals.Length == 0 || take <= 0 || fromDate > DateTime.UtcNow)
            return new List<SeqEvent>();

        var uri = CreateUri(signals, take);

        using var jsonDocument = await Request.GetAsync<JsonDocument>(uri, throwExceptionOnError: true).ConfigureAwait(false);
        var events = GetEvents(jsonDocument).Where(e => e.Date > fromDate).ToList();

        return events;
    }

    public Task<string> GetLastEvents(string queryOrFilter, int take = 10)
    {
        throw new NotImplementedException();
    }

    private static IEnumerable<SeqEvent> GetEvents(JsonDocument jsonDocument)
    {
        foreach (var jsonElement in jsonDocument.RootElement.EnumerateArray())
        {
            var eventProperties = jsonElement.EnumerateObject().ToDictionary(p => p.Name);

            yield return new SeqEvent
            {
                Date = eventProperties["Timestamp"].Value.GetDateTime(),
                Properties = eventProperties["Properties"].Value.EnumerateArray().Select(o =>
                {
                    var property = o.EnumerateObject().ToArray();
                    var name = property[0].Value.GetString();
                    var value = property[1].Value.ToString();
                    return $"{name}: {value}";
                }).ToList(),
                Messages = eventProperties["MessageTemplateTokens"].Value.EnumerateArray().SelectMany(p => p.EnumerateObject()).Select(p => p.Value.GetString()).ToList(),
                Level = eventProperties["Level"].Value.GetString(),
                LinkPart = eventProperties["Links"].Value.EnumerateObject().First().Value.GetString()
            };
        }
    }

    private string CreateUri(int[] signals, int? take = null)
    {
        var uri = signals.Length == 1
            ? $"{Url}/api/events?apikey={_token}&signal=signal-{signals[0]}"
            : $"{Url}/api/events?apikey={_token}&signal=({string.Join('~', signals.Select(s => $"signal-{s}"))})";

        return take.HasValue ? $"{uri}&count={take}" : uri;
    }
}