using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Zs.Common.Services.Connection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConnectionAnalyzer(this IServiceCollection services)
    {
        var provider = services.BuildServiceProvider();
        var configuration = provider.GetRequiredService<IConfiguration>();
        services.Configure<ConnectionAnalyzerOptions>(configuration.GetSection(ConnectionAnalyzerOptions.SectionName));

        services.AddSingleton<IConnectionAnalyzer, ConnectionAnalyzer>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<ConnectionAnalyzer>>();
            var proxy = provider.GetService<WebProxy>();
            var options = provider.GetRequiredService<IOptions<ConnectionAnalyzerOptions>>().Value;

            var connectionAnalyzer = new ConnectionAnalyzer(logger, options.Urls)
            {
                WebProxy = proxy
            };

            return connectionAnalyzer;
        });

        return services;
    }

    public static IServiceCollection AddWebProxy(this IServiceCollection services)
    {
        var provider = services.BuildServiceProvider();
        var configuration = provider.GetRequiredService<IConfiguration>();
        services.Configure<ProxyOptions>(configuration.GetSection(ProxyOptions.SectionName));

        services.AddSingleton<WebProxy>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<ProxyOptions>>().Value;

            var webProxy = new WebProxy(options.Address, true);
            if (options is {UserName: { }, Password: { }})
            {
                webProxy.Credentials = new NetworkCredential(options.UserName, options.Password);
            }

            return webProxy;
        });

        return services;
    }
}