using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Zs.Common.Extensions;
using Zs.Common.Services.Connection;

namespace IntegrationTests;

public sealed class ConnectionAnalyserTests : TestBase
{
    [Fact]
    public async Task ConnectionAnalyser_Should_UpdateStatus()
    {
        var connectionAnalyzer = ServiceProvider.GetRequiredService<IConnectionAnalyzer>();
        connectionAnalyzer.CurrentStatus.Should().Be(ConnectionStatus.Undefined);

        connectionAnalyzer.Start(0.Seconds(), 1.Seconds());
        await Task.Delay(2.Seconds());
        connectionAnalyzer.Stop();

        connectionAnalyzer.CurrentStatus.Should()
            .BeOneOf(ConnectionStatus.Ok, ConnectionStatus.NoInternetConnection);
    }
}