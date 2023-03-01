using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zs.Common.Services.Connection;

namespace IntegrationTests;

[ExcludeFromCodeCoverage]
public abstract class TestBase
{
    private const string AppsettingsFileName = "appsettings.json";
    protected readonly ServiceProvider ServiceProvider;

    protected TestBase()
    {
        ServiceProvider = CreateServiceProvider();
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddSingleton(static _ => GetAppsettings());
        services.AddMockLogger<ConnectionAnalyzer>();
        services.AddConnectionAnalyzer();

        return services.BuildServiceProvider();
    }

    private static IConfiguration GetAppsettings()
    {
        var appsettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppsettingsFileName);
        var configuration = new ConfigurationManager();
        configuration.AddJsonFile(appsettingsPath, optional: false);

        return configuration;
    }
}