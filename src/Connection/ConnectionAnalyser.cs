﻿using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using Microsoft.Extensions.Logging;
using Zs.Common.Enums;
using Zs.Common.Services.Abstractions;

namespace Zs.Common.Services.Connection;

// TODO: Make async Tasks

/// <summary> <inheritdoc/> </summary>
public sealed class ConnectionAnalyser : IConnectionAnalyser
{
    private readonly object _locker = new object();
    private readonly ILogger<ConnectionAnalyser> _logger;
    private readonly string[] _internetServers;
    private Timer _timer;

    /// <summary> <inheritdoc/> </summary>
    public WebProxy WebProxy { get; private set; }

    /// <summary> <inheritdoc/> </summary>
    public DateTime? InternetRepairDate { get; private set; }

    /// <summary> <inheritdoc/> </summary>
    public ConnectionStatus CurrentStatus { get; private set; } = ConnectionStatus.Undefined;

    /// <summary> <inheritdoc/> </summary>

    public event Action<ConnectionStatus> ConnectionStatusChanged;


    public ConnectionAnalyser(params string[] testHosts)
    {
        _internetServers = testHosts?.Length > 0 ? testHosts : throw new ArgumentException($"{nameof(testHosts)} must contains at least 1 element");
    }

    public ConnectionAnalyser(ILogger<ConnectionAnalyser> logger, params string[] testHosts)
        : this(testHosts)
    {
        _logger = logger;
    }

    /// <summary> <inheritdoc/> </summary>
    public void Start(uint dueTime, uint period)
    {
        try
        {
            _timer = new Timer(new TimerCallback(AnalyzeConnection));
            _timer.Change(dueTime, period);

            _logger?.LogInformation($"{nameof(ConnectionAnalyser)} started");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"{nameof(ConnectionAnalyser)} starting error");
        }
    }

    /// <summary> <inheritdoc/> </summary>
    public void Stop()
    {
        try
        {
            _timer.Dispose();
            _logger?.LogInformation($"{nameof(ConnectionAnalyser)} stopped");
        }
        catch (Exception ex)
        {
            _logger?.LogInformation(ex, $"{nameof(ConnectionAnalyser)} stopping error");
        }
    }

    /// <summary> <inheritdoc/> </summary>
    public void InitializeProxy(string socket, string userName = null, string password = null)
    {
        WebProxy = new WebProxy(socket, true);
        if (!string.IsNullOrWhiteSpace(userName)
            && !string.IsNullOrWhiteSpace(password))
        {
            WebProxy.Credentials = new NetworkCredential(userName, password);
            _logger?.LogInformation("Proxy used", nameof(ConnectionAnalyser));
        }
    }

    public static bool PingHost(string hostAddress)
    {
        try
        {
            hostAddress = GetClearHostAddress(hostAddress ?? throw new ArgumentNullException(nameof(hostAddress)));

            using (var ping = new Ping())
            {
                return ping.Send(hostAddress).Status == IPStatus.Success;
            }
        }
        catch
        {
            return false;
        }
    }

    public static bool ValidateIPv4(string ipString)
    {
        if (string.IsNullOrWhiteSpace(ipString))
            return false;

        var splitValues = ipString.Split('.');
        if (splitValues.Length != 4)
            return false;

        return splitValues.All(r => byte.TryParse(r, out byte tempForParsing));
    }

    private void AnalyzeConnection(object timerState)
    {
        try
        {
            if (_internetServers == null || _internetServers.Length == 0)
                throw new ArgumentException("At least one server address must be set");

            var analyzeResult = ConnectionStatus.Undefined;

            lock (_locker)
            {
                if (WebProxy?.Address?.Host is { } && !PingHost(WebProxy.Address.Host))
                    analyzeResult = ConnectionStatus.NoProxyConnection;

                if (analyzeResult == ConnectionStatus.Undefined)
                    foreach (var server in _internetServers)
                    {
                        if (PingHost(server))
                        {
                            analyzeResult = ConnectionStatus.Ok;
                            break;
                        }
                        else
                            analyzeResult = ConnectionStatus.NoInternetConnection;
                    }

                if (InternetRepairDate == null && analyzeResult == ConnectionStatus.Ok)
                    InternetRepairDate = DateTime.UtcNow;
                else if (analyzeResult != ConnectionStatus.Ok)
                    InternetRepairDate = null;

                if (analyzeResult != CurrentStatus)
                {
                    CurrentStatus = analyzeResult;
                    _logger?.LogInformation($"Connection status changed: {CurrentStatus}");

                    ConnectionStatusChanged?.Invoke(CurrentStatus);
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Connection analyze error");
        }
    }

    /// <summary> Getting host address without protocol and port </summary>
    private static string GetClearHostAddress(string hostAddress)
    {
        if (hostAddress?.Length > 0)
        {
            hostAddress = hostAddress.Contains("://", StringComparison.InvariantCulture)
                ? hostAddress.Substring(hostAddress.IndexOf("://", StringComparison.InvariantCulture) + 3)
                : hostAddress;

            hostAddress = hostAddress.Contains(":", StringComparison.InvariantCulture)
                       && !hostAddress.Contains("://", StringComparison.InvariantCulture)
                ? hostAddress.Substring(0, hostAddress.IndexOf(":", StringComparison.InvariantCulture))
                : hostAddress;

            hostAddress = hostAddress.Trim('/');
        }

        return hostAddress;
    }
}
