using System;
using System.Linq;
using System.Text;

namespace Zs.Common.Services.Logging.Seq;

internal static class Mapper
{
    public static SeqEvent ToSeqEvent(this Event @event)
    {
        var messageBuilder = new StringBuilder();
        foreach (var messagePart in @event.MessageTemplateTokens)
        {
            var part = messagePart switch
            {
                {Text: not null} => messagePart.Text,
                {RawText: not null} => messagePart.RawText,
                {FormattedValue: not null} => messagePart.FormattedValue,
                {PropertyName: not null} => @event.Properties.Single(p => p.Name == messagePart.PropertyName).Value,
                _ => throw new ArgumentOutOfRangeException(nameof(messagePart))
            };
            messageBuilder.Append(part);
        }

        return new SeqEvent
        {
            Id = @event.Id,
            Timestamp = @event.Timestamp,
            Message = messageBuilder.ToString(),
            EventType = @event.EventType,
            Level = @event.Level.ToLevel(),
            Parameters = @event.Properties.ToDictionary(p => p.Name, p => p.Value),
            Exception = @event.Exception
        };
    }

    private static string ToLevel(this string level)
    {
        return level switch
        {
            "Verbose" => "Trace",
            "Information" => "Info",
            _ => level
        };
    }
}