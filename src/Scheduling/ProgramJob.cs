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
public class ProgramJob : Job
{
    private readonly Func<Task> _method;

    public ProgramJob(
        TimeSpan period,
        Func<Task> method,
        DateTime? startUtcDate = null,
        string? description = null,
        ILogger? logger = null)
        : base(period, startUtcDate, logger)
    {
        Period = period;
        _method = method;
        Description = description;
    }

    protected override async Task<Result<string?>> GetExecutionResult()
    {
        var sw = new Stopwatch();
        sw.Start();

        await _method.Invoke().ConfigureAwait(false);

        sw.Stop();
        Logger?.LogTraceIfNeed("Job {Job} done. Elapsed: {Elapsed}", Description, sw.Elapsed);

        return Result.Success<string?>(null);
    }
}