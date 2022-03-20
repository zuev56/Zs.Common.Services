using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zs.Common.Abstractions;
using Zs.Common.Models;

namespace Zs.Common.Services.Scheduler;

/// <summary>
/// <see cref="Job"/> based on program code
/// </summary>
public sealed class ProgramJob : Job
{
    private readonly Func<Task> _method;
    private readonly object _parameter;

    public ProgramJob(
        TimeSpan period,
        Func<Task> method,
        object parameter = null,
        DateTime? startUtcDate = null,
        string description = null,
        ILogger logger = null)
        : base(period, startUtcDate, logger)
    {
        Period = period != default ? period : throw new ArgumentException($"{nameof(period)} can't have default value");
        _method = method ?? throw new ArgumentNullException(nameof(method));
        _parameter = parameter;
        Description = description;
    }

    protected override async Task<IOperationResult<string>> GetExecutionResult()
    {
        var sw = new Stopwatch();
        sw.Start();

        await _method.Invoke().ConfigureAwait(false);

        sw.Stop();
        _logger?.LogTrace($"Job {{Job}} done. Elapsed: {{Elapsed}}", Description, sw.Elapsed);

        return ServiceResult<string>.Success(default);
    }
}

/// <summary>
/// <see cref="Job"/> based on program code
/// </summary>
public sealed class ProgramJob<TResult> : Job<TResult>
{
    private readonly Func<Task<TResult>> _method;
    private readonly object _parameter;

    // TODO: Use BUILDER to create instanses
    public ProgramJob(
        Func<Task<TResult>> method,
        TimeSpan period,
        DateTime? startUtcDate = null,
        object parameter = null,
        string description = null,
        ILogger logger = null)
        : base(period, startUtcDate, logger)
    {
        Period = period != default ? period : throw new ArgumentException($"{nameof(period)} can't have default value");
        _method = method ?? throw new ArgumentNullException(nameof(method));
        _parameter = parameter;
        Description = description;
    }

    protected override async Task<IOperationResult<TResult>> GetExecutionResult()
    {
        var sw = new Stopwatch();
        sw.Start();

        TResult result = await _method.Invoke().ConfigureAwait(false);

        sw.Stop();
        _logger?.LogTrace($"Job {{Job}} done. Elapsed: {{Elapsed}}", Description, sw.Elapsed);

        return ServiceResult<TResult>.Success(result);
    }
}
