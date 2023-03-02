using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Zs.Common.Extensions;
using Zs.Common.Services.Scheduling;

namespace UnitTests.Scheduler
{
    public sealed class SchedulerTests
    {
        private const int ProgramJobResultValue = 12345;

        [Fact]
        public async Task Scheduler_Should_RunJobMultipleTimes()
        {
            var period = 100.Milliseconds();
            var job = CreateProgramJob(period);
            var scheduler = new Zs.Common.Services.Scheduling.Scheduler();
            scheduler.Jobs.Add(job);
            var utcNow = DateTime.UtcNow;

            scheduler.Start(0.Seconds(), period);
            await Task.Delay(period * 10);
            scheduler.Stop();

            job.Counter.Should().BeInRange(5, 10);
            job.LastRunUtcDate.Should().NotBeNull();
            job.LastRunUtcDate!.Value.Should().BeAfter(utcNow);
            job.LastResult.Should().NotBeNull();
            job.LastResult!.Value.Should().Be(ProgramJobResultValue);
            job.NextRunUtcDate.Should().Be(job.LastRunUtcDate + period);
            job.IdleStepsCount.Should().Be(0);
        }

        [Fact]
        public async Task Scheduler_Should_NotRunJobWithIdleSteps()
        {
            var period = 100.Milliseconds();
            var job = CreateProgramJob(period);
            job.IdleStepsCount = 10;
            var scheduler = new Zs.Common.Services.Scheduling.Scheduler();
            scheduler.Jobs.Add(job);

            scheduler.Start(0.Seconds(), period);
            await Task.Delay(period * 10);
            scheduler.Stop();

            job.Counter.Should().BeInRange(5, 10);
            job.LastRunUtcDate.Should().NotBeNull();
            job.LastResult.Should().BeNull();
            job.IdleStepsCount.Should().BeInRange(0, 5);
        }

        private static ProgramJob<int> CreateProgramJob(TimeSpan period)
        {
            var startDate = (DateTime?)DateTime.UtcNow;
            var method = static () => Task.FromResult(ProgramJobResultValue);
            var job = new ProgramJob<int>(method, period, startDate);

            job.Counter.Should().Be(0);
            job.LastRunUtcDate.Should().BeNull();
            job.NextRunUtcDate.Should().Be(startDate);
            job.LastResult.Should().BeNull();
            job.IdleStepsCount.Should().Be(0);

            return job;
        }

    }
}