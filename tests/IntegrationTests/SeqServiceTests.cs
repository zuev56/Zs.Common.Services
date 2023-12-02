using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Zs.Common.Services.Logging.Seq;

namespace IntegrationTests;

public sealed class SeqServiceTests : TestBase
{
    [Fact]
    public async Task GetLastEventsAsync_SignalsNotSpecified_ReturnsSeqEvents()
    {
        var seqService = CreateSeqService();
        var take = 1000;

        var seqEvents = await seqService.GetLastEventsAsync(take);

        seqEvents.Should().NotBeNull()
            .And.HaveCount(take);
        // TODO
    }

    private SeqService CreateSeqService()
    {
        var configuration = ServiceProvider.GetRequiredService<IConfiguration>();
        var url = configuration.GetValue<string>("Seq:Url")!;
        var token = configuration.GetValue<string>("Seq:Token")!;
        var seqService = new SeqService(url, token);
        return seqService;
    }
}