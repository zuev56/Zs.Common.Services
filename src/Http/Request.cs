using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zs.Common.Extensions;

namespace Zs.Common.Services.Http;

public sealed class Request
{
    private readonly string _uri;
    private readonly IHttpClientFactory? _httpClientFactory;
    private Dictionary<string, string?>? _headers;
    private ILogger? _logger;

    private Request(string uri, IHttpClientFactory? httpClientFactory)
    {
        _uri = uri;
        _httpClientFactory = httpClientFactory;
    }

    public static Request Create(string uri, IHttpClientFactory? httpClientFactory = null)
    {
        return new Request(uri, httpClientFactory);
    }

    public Request WithHeaders(Dictionary<string, string?> headers)
    {
        _headers = headers;
        return this;
    }

    public Request WithLogger(ILogger logger)
    {
        _logger = logger;
        return this;
    }

    public async Task<TResult?> GetAsync<TResult>(CancellationToken cancellationToken = default)
    {
        var stopWatch = Stopwatch.StartNew();
        using var httpClient = _httpClientFactory != null ? _httpClientFactory.CreateClient() : new HttpClient();
        SetHeaders(httpClient);

        TResult? result;
        try
        {
            var stream = await httpClient.GetStreamAsync(_uri, cancellationToken).ConfigureAwait(false);
            result = await JsonSerializer.DeserializeAsync<TResult>(stream, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger?.LogErrorIfNeed(ex, "GET {uri} {time} ms", _uri, stopWatch.ElapsedMilliseconds);
            throw;
        }

        _logger?.LogTraceIfNeed("GET {uri} {time} ms", _uri, stopWatch.ElapsedMilliseconds);
        return result;
    }

    private void SetHeaders(HttpClient httpClient)
    {
        if (_headers == null || !_headers.Any())
            return;

        httpClient.DefaultRequestHeaders.Clear();
        foreach (var header in _headers)
        {
            var (name, value) = header;
            httpClient.DefaultRequestHeaders.Add(name, value);
        }
    }
}