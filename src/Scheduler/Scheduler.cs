using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Zs.Common.Extensions;
using Zs.Common.Services.Abstractions;

namespace Zs.Common.Services.Scheduler
{
    public sealed class Scheduler : IScheduler
    {
        private readonly ILogger<Scheduler> _logger;
        private Timer _timer;

        public List<IJobBase> Jobs { get; } = new List<IJobBase>();

        public Scheduler(ILogger<Scheduler> logger = null)
        {
            _logger = logger;
        }

        public void Start(uint dueTimeMs, uint periodMs)
        {
            try
            {
                _timer = new Timer(new TimerCallback(DoWork));
                _timer.Change(dueTimeMs, periodMs);

                _logger?.LogInformation($"{nameof(Scheduler)} started");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"{nameof(Scheduler)} starting error");
            }
        }

        public void Stop()
        {
            try
            {
                _timer.Dispose();
                _logger?.LogInformation($"{nameof(Scheduler)} stopped");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"{nameof(Scheduler)} stopping error");
            }
        }

        private void DoWork(object state)
        {
            try
            {
                foreach (var job in Jobs)
                {
                    if (!job.IsRunning && (job.NextRunUtcDate == null || job.NextRunUtcDate < DateTime.UtcNow))
                    {
                        Task.Run(async () => await job.Execute())
                            .ContinueWith((task) =>
                            {
                                foreach (var exeption in task.Exception.InnerExceptions)
                                    _logger.LogErrorIfNeed(exeption, "Job \"{JobDescription}\" executing error", job.Description);
                            }, TaskContinuationOptions.OnlyOnFaulted);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogErrorIfNeed(ex, $"{nameof(Scheduler)} DoWork error");
            }
        }
    }
}
