using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zs.Common.Extensions;

namespace Zs.Common.Services.Scheduling;

public sealed class Scheduler : IScheduler
{
    private readonly ILogger<Scheduler>? _logger;
    private Timer _timer = null!;

    public List<JobBase> Jobs { get; } = new();

    public Scheduler(ILogger<Scheduler>? logger = null)
    {
        _logger = logger;
    }

    public void Start(TimeSpan dueTime, TimeSpan period)
    {
        _timer = new Timer(DoWork);
        _timer.Change(dueTime, period);

        _logger?.LogInformation($"{nameof(Scheduler)} started");
    }

    public void Stop()
    {
        _timer.Dispose();

        _logger?.LogInformation($"{nameof(Scheduler)} stopped");
    }

    private void DoWork(object? state)
    {
        try
        {
            var readyToRunJobs = Jobs
                .Where(static job => !job.IsRunning && (job.NextRunUtcDate == null || job.NextRunUtcDate < DateTime.UtcNow));

            foreach (var job in readyToRunJobs)
            {
                // TODO: maybe run parallel?
                Task.Run(async () => await job.ExecuteAsync().ConfigureAwait(false))
                    .ContinueWith(task =>
                    {
                        foreach (var exception in task.Exception!.InnerExceptions)
                        {
                            _logger?.LogErrorIfNeed(exception, "Job \"{JobDescription}\" executing error", job.Description);
                        }
                    }, TaskContinuationOptions.OnlyOnFaulted);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogErrorIfNeed(ex, $"{nameof(Scheduler)} DoWork error");
        }
    }
}