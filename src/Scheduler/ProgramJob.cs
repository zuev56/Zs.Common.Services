using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zs.Common.Models;

namespace Zs.Common.Services.Scheduler;

/// <summary>
/// <see cref="Job"/> based on program code
/// </summary>
public sealed class ProgramJob : Job
{
    private readonly Func<Task> _method;
    private readonly object? _parameter;

    public ProgramJob(
        TimeSpan period,
        Func<Task> method,
        object? parameter = null,
        DateTime? startUtcDate = null,
        string? description = null,
        ILogger? logger = null)
        : base(period, startUtcDate, logger)
    {
        Period = period;
        _method = method;
        _parameter = parameter;
        Description = description;
    }

    protected override async Task<Result<string?>> GetExecutionResult()
    {
        var sw = new Stopwatch();
        sw.Start();

        await _method.Invoke().ConfigureAwait(false);

        sw.Stop();
        Logger?.LogTrace("Job {Job} done. Elapsed: {Elapsed}", Description, sw.Elapsed);

        return Result.Success<string?>(null);
    }
}

/// <summary>
/// <see cref="Job"/> based on program code
/// </summary>
public sealed class ProgramJob<TResult> : Job<TResult>
{
    private readonly Func<Task<TResult>> _method;
    private readonly object? _parameter;

    // TODO: Use BUILDER to create instanses
    public ProgramJob(
        Func<Task<TResult>> method,
        TimeSpan period,
        DateTime? startUtcDate = null,
        object? parameter = null,
        string? description = null,
        ILogger? logger = null)
        : base(period, startUtcDate, logger)
    {
        Period = period;
        _method = method;
        _parameter = parameter;
        Description = description;
    }

    protected override async Task<Result<TResult>> GetExecutionResult()
    {
        var sw = new Stopwatch();
        sw.Start();

        var result = await _method.Invoke().ConfigureAwait(false);

        sw.Stop();
        Logger?.LogTrace("Job {Job} done. Elapsed: {Elapsed}", Description, sw.Elapsed);

        return Result<TResult>.Success(result);
    }
}