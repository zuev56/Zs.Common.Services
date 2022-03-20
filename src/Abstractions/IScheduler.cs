using System.Collections.Generic;

namespace Zs.Common.Services.Abstractions;

public interface IScheduler
{
    /// <summary> Job list </summary>
    List<IJobBase> Jobs { get; }

    /// <summary> Start scheduler instance </summary>
    /// <param name="dueTimeMs">The amount of time to delay before starting</param>
    /// <param name="periodMs">The time interval between iterations</param>
    void Start(uint dueTimeMs, uint periodMs);

    /// <summary> Stop scheduler instance </summary>
    void Stop();
}
