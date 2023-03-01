using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace IntegrationTests;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMockLogger<T>(this IServiceCollection services)
    {
        services.AddSingleton<ILogger<T>>(static _ =>
        {
            var mock = new Mock<ILogger<T>>();
            return mock.Object;
        });

        return services;
    }
}