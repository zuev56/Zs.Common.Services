using System;
using System.Collections.Generic;

namespace Zs.Common.Services.Scheduling;

public interface IScheduler
{
    /// <summary> Job list </summary>
    List<JobBase> Jobs { get; }

    /// <summary> Start scheduler instance </summary>
    /// <param name="dueTime">The amount of time to delay before start</param>
    /// <param name="period">The time interval between iterations</param>
    void Start(TimeSpan dueTime, TimeSpan period);

    /// <summary> Stop scheduler instance </summary>
    void Stop();
}