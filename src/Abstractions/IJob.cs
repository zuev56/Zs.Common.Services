using System;
using Zs.Common.Abstractions;

namespace Zs.Common.Services.Abstractions
{
    public interface IJob : IJobBase
    {
        IOperationResult LastResult { get; }

        event Action<IJob, IOperationResult> ExecutionCompleted;
    }

    public interface IJob<TResult> : IJobBase
    {
        IOperationResult<TResult> LastResult { get; }

        event Action<IJob<TResult>, IOperationResult<TResult>> ExecutionCompleted;
    }
}
