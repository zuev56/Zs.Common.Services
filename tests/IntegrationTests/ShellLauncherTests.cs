using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Zs.Common.Services.Shell;

namespace IntegrationTests;

public sealed class ShellLauncherTests : TestBase
{
    private const string GetDateCommand = "date +\"%d-%m-%y\"";
    private readonly string _currentDate = DateTime.Now.ToString("dd-MM-yy");

    [Fact]
    public async Task ShellLauncher_Should_SuccessfullyLaunchBashCommand()
    {
        var configuration = ServiceProvider.GetRequiredService<IConfiguration>();
        var bashPath = configuration["ShellLauncher:BashPath"]!;

        var result = await ShellLauncher.RunAsync(bashPath, GetDateCommand);

        result.Successful.Should().BeTrue();
        result.Value.Trim().Should().Be(_currentDate);
    }

    [Fact]
    public async Task ShellLauncher_Should_SuccessfullyLaunchPowerShellCommand()
    {
        var configuration = ServiceProvider.GetRequiredService<IConfiguration>();
        var powershellPath = configuration["ShellLauncher:PowerShellPath"]!;

        var result = await ShellLauncher.RunAsync(powershellPath, GetDateCommand);

        result.Successful.Should().BeTrue();
        result.Value.Trim().Should().Be(_currentDate);
    }
}