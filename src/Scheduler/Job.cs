using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Zs.Common.Abstractions;
using Zs.Common.Services.Abstractions;

namespace Zs.Common.Services.Scheduler
{
    /// <summary>
    /// A specified series of operations performed sequentially by <see cref="Scheduler"/>
    /// </summary>
    public abstract class Job : JobBase, IJob
    {
        public IOperationResult LastResult { get; protected set; }

        public event Action<IJob, IOperationResult> ExecutionCompleted;

        /// <summary>  </summary>
        /// <param name="period">Time interval between executions</param>
        /// <param name="startDate">First execution date</param>
        public Job(TimeSpan period, DateTime? startDate = null, ILogger logger = null)
            : base(period, startDate, logger)
        {
        }

        protected abstract Task<IOperationResult<string>> GetExecutionResult();

        protected override async Task AfterExecution()
        {
            LastResult = await GetExecutionResult().ConfigureAwait(false);
            Volatile.Read(ref ExecutionCompleted)?.Invoke(this, LastResult);
        }
    }

    /// <summary>
    /// A specified series of operations performed sequentially by <see cref="Scheduler"/>
    /// </summary>
    public abstract class Job<TResult> : JobBase, IJob<TResult>
    {
        public IOperationResult<TResult> LastResult { get; protected set; }

        public event Action<IJob<TResult>, IOperationResult<TResult>> ExecutionCompleted;

        /// <summary>  </summary>
        /// <param name="period">Time interval between executions</param>
        /// <param name="startDate">First execution date</param>
        public Job(TimeSpan period, DateTime? startDate = null, ILogger logger = null)
            : base(period, startDate, logger)
        {
        }

        protected abstract Task<IOperationResult<TResult>> GetExecutionResult();
        
        protected override async Task AfterExecution()
        {
            LastResult = await GetExecutionResult().ConfigureAwait(false);
            Volatile.Read(ref ExecutionCompleted)?.Invoke(this, LastResult);
        }
    }

}
