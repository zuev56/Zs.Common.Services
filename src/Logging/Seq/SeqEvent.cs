using System;
using System.Collections.Generic;

namespace Zs.Common.Services.Logging.Seq;

public sealed class SeqEvent
{
    public DateTime Date { get; init; }
    public List<string> Properties { get; init; } = new ();
    public List<string> Messages { get; init; } = new ();
    public string Level { get; set; } = null!;
    public string LinkPart { get; set; } = null!;
}