using System;
using System.Net;

namespace Zs.Common.Services.Connection;

/// <summary>
/// Analyzes internet connection
/// </summary>
public interface IConnectionAnalyzer
{
    /// <summary>Current connection status</summary>
    ConnectionStatus CurrentStatus { get; }

    /// <summary>The date when internet connection was repaired</summary>
    DateTime? InternetRepairDate { get; }

    /// <summary>
    /// <see cref="System.Net.WebProxy"/> instance
    /// </summary>
    WebProxy WebProxy { get; }

    /// <summary>Occurs when connection status changes</summary>
    event Action<ConnectionStatus> ConnectionStatusChanged;

    /// <summary>Starts the <see cref="IConnectionAnalyzer"/></summary>
    /// <param name="dueTime">The amount of time to delay before start</param>
    /// <param name="period">The time interval between iterations</param>
    void Start(TimeSpan dueTime, TimeSpan period);

    /// <summary>Stops the <see cref="IConnectionAnalyzer"/></summary>
    void Stop();
}