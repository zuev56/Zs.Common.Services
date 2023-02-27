using System;
using Microsoft.Extensions.Logging;

namespace Zs.Common.Services.Logging.DelayedLogger;

public interface IDelayedLogger<TSourceContext>
{
    TimeSpan DefaultLogWriteInterval { get; set; }
    void SetupLogMessage(string messageText, TimeSpan logShowInterval);
    int Log(string messageText, LogLevel logLevel);
    int LogTrace(string messageText);
    int LogDebug(string messageText);
    int LogInformation(string messageText);
    int LogWarning(string messageText);
    int LogError(string messageText);
    int LogCritical(string messageText);
}