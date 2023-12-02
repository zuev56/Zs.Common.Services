using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zs.Common.Services.Logging.Seq;

public interface ISeqService
{
    Task<IReadOnlyList<SeqEvent>> GetLastEventsAsync(int take, params int[] signals);
}