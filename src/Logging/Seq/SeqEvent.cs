using System;
using System.Collections.Generic;

namespace Zs.Common.Services.Logging.Seq
{
    public sealed class SeqEvent
    {
        public DateTime Date { get; init; }
        public List<string> Properties { get; init; }
        public List<string> Messages { get; init; }
        public string Level { get; set; }
        public string LinkPart { get; set; }
    }
}
