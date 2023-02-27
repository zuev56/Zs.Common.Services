using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zs.Common.Services.Logging.Seq;

public interface ISeqService
{
    public string Url { get; }
    Task<List<SeqEvent>> GetLastEventsAsync(int take = 10, params int[] signals);
    Task<List<SeqEvent>> GetLastEventsAsync(DateTime fromDate, int take = 10, params int[] signals);
}