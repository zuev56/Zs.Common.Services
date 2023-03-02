using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zs.Common.Models;

namespace Zs.Common.Services.Scheduling;

/// <summary>
/// A specified series of operations performed sequentially by <see cref="Scheduler"/>
/// </summary>
public abstract class Job<TResult> : JobBase
{
    public Result<TResult>? LastResult { get; protected set; }

    public event Action<Job<TResult>, Result<TResult>>? ExecutionCompleted;

    /// <summary>  </summary>
    /// <param name="period">Time interval between executions</param>
    /// <param name="startDate">First execution date</param>
    /// <param name="logger">Instance of logger</param>
    public Job(TimeSpan period, DateTime? startDate = null, ILogger? logger = null)
        : base(period, startDate, logger)
    {
    }

    protected abstract Task<Result<TResult>> GetExecutionResult();

    protected override async Task AfterExecution()
    {
        LastResult = await GetExecutionResult().ConfigureAwait(false);
        Volatile.Read(ref ExecutionCompleted)?.Invoke(this, LastResult);
    }
}