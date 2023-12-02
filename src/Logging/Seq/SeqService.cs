using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zs.Common.Exceptions;
using Zs.Common.Services.Http;
using static Zs.Common.Services.Logging.Seq.FaultCodes;

namespace Zs.Common.Services.Logging.Seq;

public sealed class SeqService : ISeqService
{
    private readonly string _url;
    private readonly Dictionary<string, string?> _headers = new();
    private readonly ILogger<SeqService>? _logger;

    public SeqService(string url, string token, ILogger<SeqService>? logger = null)
    {
        _url = url.TrimEnd('/');
        _headers.Add("X-Seq-ApiKey", token);
        _logger = logger;
    }

    public async Task<IReadOnlyList<SeqEvent>> GetLastEventsAsync(int take, params int[] signals)
    {
        if (take <= 0)
            return new List<SeqEvent>();

        var uri = CreateUri(take, signals);
        var request = Request.Create(uri).WithHeaders(_headers);
        if (_logger != null)
            request = request.WithLogger(_logger);

        var events = await request.GetAsync<List<Event>>().ConfigureAwait(false);
        if (events == null)
            return new List<SeqEvent>();

        EnsureEventsValid(events);

        return events.Select(Mapper.ToSeqEvent).ToList();
    }

    private static void EnsureEventsValid(List<Event> events)
    {
        var areValid = events.AsParallel().All(e =>
            !string.IsNullOrWhiteSpace(e.Id)
         && !string.IsNullOrWhiteSpace(e.EventType)
         && !string.IsNullOrWhiteSpace(e.Level)
         && e.MessageTemplateTokens.All(t =>
                t.Text != null || t.RawText != null || t.FormattedValue != null || t.PropertyName != null)
        );

        if (!areValid)
            throw new FaultException(InvalidSeqEvents);
    }

    private string CreateUri(int take, IReadOnlyList<int> signals)
    {
        return signals.Count switch
        {
            0 => $"{_url}/api/events?count={take}",
            1 => $"{_url}/api/events?count={take}&signal=signal-{signals[0]}",
            _ => $"{_url}/api/events?count={take}&signal=({string.Join('~', signals.Select(s => $"signal-{s}"))})"
        };
    }
}