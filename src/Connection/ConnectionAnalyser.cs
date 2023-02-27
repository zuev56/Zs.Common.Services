using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zs.Common.Enums;

namespace Zs.Common.Services.Connection;

public sealed class ConnectionAnalyser : IConnectionAnalyser
{
    private readonly object _locker = new();
    private readonly ILogger<ConnectionAnalyser>? _logger;
    private readonly string[] _internetServers;
    private Timer _timer = null!;

    public WebProxy? WebProxy { get; private set; }
    public DateTime? InternetRepairDate { get; private set; }
    public ConnectionStatus CurrentStatus { get; private set; } = ConnectionStatus.Undefined;

    public event Action<ConnectionStatus>? ConnectionStatusChanged;

    public ConnectionAnalyser(params string[] testHosts)
    {
        _internetServers = testHosts.Length > 0 ? testHosts : throw new ArgumentException($"{nameof(testHosts)} must contains at least 1 element");
    }

    public ConnectionAnalyser(ILogger<ConnectionAnalyser> logger, params string[] testHosts)
        : this(testHosts)
    {
        _logger = logger;
    }

    public void Start(TimeSpan dueTime, TimeSpan period)
    {
        _timer = new Timer(AnalyzeConnection);
        _timer.Change(dueTime, period);

        _logger?.LogInformation("{Service} started", nameof(ConnectionAnalyser));
    }

    public void Stop()
    {
        _timer.Dispose();

        _logger?.LogInformation("{Service} stopped", nameof(ConnectionAnalyser));
    }

    public void InitializeProxy(string socket, string? userName = null, string? password = null)
    {
        ArgumentNullException.ThrowIfNull(socket);

        WebProxy = new WebProxy(socket, true);
        if (userName != null && password != null)
        {
            WebProxy.Credentials = new NetworkCredential(userName, password);
            _logger?.LogInformation($"Use proxy '{socket}'");
        }
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

    public static bool PingHost(string hostAddress)
    {
        ArgumentNullException.ThrowIfNull(hostAddress);

        try
        {
            hostAddress = GetClearHostAddress(hostAddress);

            using var ping = new Ping();
            return ping.Send(hostAddress).Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }

    public static bool ValidateIPv4(string ipString)
    {
        if (string.IsNullOrWhiteSpace(ipString))
        {
            return false;
        }

        var splitValues = ipString.Split('.');
        return splitValues.Length == 4 && splitValues.All(r => byte.TryParse(r, out _));
    }

    private void AnalyzeConnection(object? timerState)
    {
        try
        {
            if (_internetServers == null || _internetServers.Length == 0)
            {
                throw new ArgumentException("At least one server address must be set");
            }

            var analyzeResult = ConnectionStatus.Undefined;

            lock (_locker)
            {
                if (WebProxy?.Address?.Host is { } && !PingHost(WebProxy.Address.Host))
                {
                    analyzeResult = ConnectionStatus.NoProxyConnection;
                }

                if (analyzeResult == ConnectionStatus.Undefined)
                    foreach (var server in _internetServers)
                    {
                        if (PingHost(server))
                        {
                            analyzeResult = ConnectionStatus.Ok;
                            break;
                        }

                        analyzeResult = ConnectionStatus.NoInternetConnection;
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