using System;
using System.Collections.Generic;

namespace Zs.Common.Services.Logging.Seq;

public sealed record SeqEvent
{
    public required string Id { get; init; } = null!;
    public required string Message { get; init; } = null!;
    public required DateTime Timestamp { get; init; }
    public required string EventType { get; init; } = null!;
    public required string Level { get; init; } = null!;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public string? Exception { get; init; }
}