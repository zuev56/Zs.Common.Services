using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Zs.Common.Services.Scheduling;

public abstract class JobBase
{
    private int _isRunning;
    private int _idleStepsCount;
    private const int Stopped = 0, Running = 1;
    protected readonly ILogger? Logger;

    public virtual bool IsRunning => _isRunning == Running;
    public virtual int IdleStepsCount
    {
        get => _idleStepsCount;
        set => Interlocked.Exchange(ref _idleStepsCount, value);
    }
    public long Counter { get; protected set; }
    public DateTime? NextRunUtcDate { get; protected set; }
    public DateTime? LastRunUtcDate { get; protected set; }
    public TimeSpan Period { get; protected set; }
    public string? Description { get; init; }

    /// <summary>  </summary>
    /// <param name="period">Time interval between executions</param>
    /// <param name="startDate">First execution date</param>
    /// <param name="logger">Instance of logger</param>
    protected JobBase(TimeSpan period, DateTime? startDate = null, ILogger? logger = null)
    {
        Period = period;
        NextRunUtcDate = startDate;
        Logger = logger;
    }

    public Task ExecuteAsync()
    {
        if (Interlocked.Exchange(ref _isRunning, Running) != Stopped)
        {
            return Task.CompletedTask;
        }

        try
        {
            if (_idleStepsCount > 0)
            {
                Interlocked.Decrement(ref _idleStepsCount);
                return Task.CompletedTask;
            }

            if (Period == default)
            {
                NextRunUtcDate = LastRunUtcDate = DateTime.MaxValue;
                Period = TimeSpan.MaxValue;
                throw new ArgumentException($"{nameof(Period)} can't have default value");
            }

            return AfterExecution();
        }
        finally
        {
            var utcNow = DateTime.UtcNow;
            LastRunUtcDate = utcNow;
            NextRunUtcDate = utcNow + Period;
            Counter++;
            Interlocked.Exchange(ref _isRunning, Stopped);
        }
    }

    protected abstract Task AfterExecution();
}