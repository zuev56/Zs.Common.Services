using System;
using System.Collections.Generic;

namespace Zs.Common.Services.Logging.Seq;

internal sealed class Event
{
    public DateTime Timestamp { get; init; }
    public IReadOnlyList<Property> Properties { get; init; } = Array.Empty<Property>();
    public IReadOnlyList<MessageTemplateToken> MessageTemplateTokens { get; init; } = Array.Empty<MessageTemplateToken>();
    public string EventType { get; init; } = null!;
    public string Level { get; init; } = null!;
    public string Id { get; init; } = null!;
    public Links Links { get; init; } = null!;
    public string? Exception { get; init; }
}

internal sealed class MessageTemplateToken
{
    public string? Text { get; init; }
    public string? PropertyName { get; init; }
    public string? RawText { get; init; }
    public string? FormattedValue { get; init; }
}

internal sealed class Property
{
    public string Name { get; init; } = null!;
    public object Value { get; init; } = null!;
}

internal sealed class Links
{
    public string Self { get; init; } = null!;
    public string Group { get; init; } = null!;
}