using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Zs.Common.Services.Connection;

public sealed class ConnectionAnalyzer : IConnectionAnalyzer
{
    private readonly ILogger<ConnectionAnalyzer>? _logger;
    private readonly string[] _internetServers;
    private Timer _timer = null!;

    public WebProxy? WebProxy { get; set; }
    public DateTime? InternetRepairDate { get; private set; }
    public ConnectionStatus CurrentStatus { get; private set; } = ConnectionStatus.Undefined;

    public event Action<ConnectionStatus>? ConnectionStatusChanged;

    public ConnectionAnalyzer(params string[] testHosts)
    {
        _internetServers = testHosts.Length > 0 ? testHosts : throw new ArgumentException($"{nameof(testHosts)} must contains at least 1 element");
    }

    public ConnectionAnalyzer(ILogger<ConnectionAnalyzer> logger, params string[] testHosts)
        : this(testHosts)
    {
        _logger = logger;
    }

    public void Start(TimeSpan dueTime, TimeSpan period)
    {
        _timer = new Timer(AnalyzeConnection);
        _timer.Change(dueTime, period);

        _logger?.LogInformation("{Service} started", nameof(ConnectionAnalyzer));
    }

    public void Stop()
    {
        _timer.Dispose();

        _logger?.LogInformation("{Service} stopped", nameof(ConnectionAnalyzer));
    }

    public static async Task<bool> PingHostAsync(string hostAddress)
    {
        ArgumentNullException.ThrowIfNull(hostAddress);

        try
        {
            hostAddress = GetClearHostAddress(hostAddress);

            using var ping = new Ping();
            var pingReply = await ping.SendPingAsync(hostAddress).ConfigureAwait(false);

            return pingReply.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }

    private async void AnalyzeConnection(object? timerState)
    {
        try
        {
            var analyzeResult = ConnectionStatus.Undefined;

            if (WebProxy?.Address?.Host is { } && !await PingHostAsync(WebProxy.Address.Host))
            {
                analyzeResult = ConnectionStatus.NoProxyConnection;
            }

            if (analyzeResult == ConnectionStatus.Undefined)
            {
                foreach (var server in _internetServers)
                {
                    var isAvailable = await PingHostAsync(server);
                    if (isAvailable)
                    {
                        analyzeResult = ConnectionStatus.Ok;
                        break;
                    }

                    analyzeResult = ConnectionStatus.NoInternetConnection;
                }
            }

            if (InternetRepairDate == null && analyzeResult == ConnectionStatus.Ok)
            {
                InternetRepairDate = DateTime.UtcNow;
            }
            else if (analyzeResult != ConnectionStatus.Ok)
            {
                InternetRepairDate = null;
            }

            if (analyzeResult != CurrentStatus)
            {
                CurrentStatus = analyzeResult;
                _logger?.LogInformation("Connection status changed: {CurrentStatus}", CurrentStatus);

                ConnectionStatusChanged?.Invoke(CurrentStatus);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Connection analyze error");
        }
    }

    /// <summary>Get host address without protocol and port</summary>
    private static string GetClearHostAddress(string hostAddress)
    {
        if (hostAddress.Length <= 0)
        {
            return hostAddress;
        }

        var indexOfColonAndDoubleSlashes = hostAddress.IndexOf("://", StringComparison.InvariantCulture);

        hostAddress = indexOfColonAndDoubleSlashes != -1
            ? hostAddress[(indexOfColonAndDoubleSlashes + 3)..]
            : hostAddress;

        var indexOfColon = hostAddress.IndexOf(":", StringComparison.InvariantCulture);

        hostAddress = indexOfColon != -1 && indexOfColonAndDoubleSlashes == -1
            ? hostAddress[..indexOfColon]
            : hostAddress;

        hostAddress = hostAddress.Trim('/');

        return hostAddress;
    }
}