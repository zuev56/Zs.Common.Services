using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zs.Common.Services.Abstractions;
using Zs.Common.Services.WebAPI;

namespace Zs.Common.Services.Logging.Seq;

/// <summary>Provides access to Seq's log</summary>
public sealed class SeqService : ISeqService
{
    private readonly string _token;
    private readonly ILogger<SeqService> _logger;

    public string Url { get; } // Example: http://localhost:5341

    public SeqService(string url, string token, ILogger<SeqService> logger = null)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentNullException(nameof(url));

        Url = url.TrimEnd('/');
        _token = token ?? throw new ArgumentNullException(nameof(token));

        _logger = logger;
    }

    public async Task<List<SeqEvent>> GetLastEvents(int take = 10, params int[] signals)
        => await GetLastEvents(DateTime.MinValue, take, signals).ConfigureAwait(false);

    public async Task<List<SeqEvent>> GetLastEvents(DateTime fromDate, int take = 10, params int[] signals)
    {
        if (signals == null || signals.Length == 0 || take <= 0 || fromDate > DateTime.UtcNow)
            return new List<SeqEvent>();

        var uri = CreateUri(signals, take);

        using (var jsonDocument = await ApiHelper.GetAsync<JsonDocument>(uri, throwExceptionOnError: true))
        {
            return GetEvents(jsonDocument).Where(e => e.Date > fromDate).ToList();
        }
    }

    public Task<string> GetLastEvents(string queryOrFilter, int take = 10)
    {
        throw new NotImplementedException();
    }

    private IEnumerable<SeqEvent> GetEvents(JsonDocument jsonDocument)
    {
        foreach (var jsonElement in jsonDocument.RootElement.EnumerateArray())
        {
            var eventProperties = jsonElement.EnumerateObject().ToDictionary(p => p.Name);

            yield return new SeqEvent
            {
                Date = eventProperties["Timestamp"].Value.GetDateTime(),
                Properties = eventProperties["Properties"].Value.EnumerateArray().Select(o =>
                {
                    var property = o.EnumerateObject().ToArray(); // [0] - Name, [1] - Value
                    var propertyName = property[0].Value.GetString();
                    var propertyValue = property[1].Value.ToString();
                    return $"{propertyName}: {propertyValue}";
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

