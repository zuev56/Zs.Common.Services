using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zs.Common.Extensions;
using Zs.Common.Models;

namespace Zs.Common.Services.Scheduling;

/// <summary>
/// <see cref="Job"/> based on program code
/// </summary>
public sealed class ProgramJob<TResult> : Job<TResult>
{
    private readonly Func<Task<TResult>> _method;

    // TODO: Use BUILDER to create instanses
    public ProgramJob(
        Func<Task<TResult>> method,
        TimeSpan period,
        DateTime? startUtcDate = null,
        string? description = null,
        ILogger? logger = null)
        : base(period, startUtcDate, logger)
    {
        Period = period;
        _method = method;
        Description = description;
    }

    protected override async Task<Result<TResult>> GetExecutionResult()
    {
        var sw = new Stopwatch();
        sw.Start();

        var result = await _method.Invoke().ConfigureAwait(false);

        sw.Stop();
        Logger?.LogTraceIfNeed("Job {Job} done. Elapsed: {Elapsed}", Description, sw.Elapsed);

        return Result.Success(result);
    }
}