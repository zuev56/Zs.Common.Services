using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zs.Common.Services.Logging.Seq;

namespace Zs.Common.Services.Abstractions
{
    public interface ISeqService
    {
        public string Url { get; }
        Task<List<SeqEvent>> GetLastEvents(int take = 10, params int[] signals);
        Task<List<SeqEvent>> GetLastEvents(DateTime fromDate, int take = 10, params int[] signals);
    }
}