using Domain.Runtime.Environment.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Web.Extension;

public static class WebExtensions
{
    public static IServiceCollection AddWebExtensions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var merged = new ConfigurationBuilder()
            .AddConfiguration(configuration)
            .AddEnvironmentVariables("TOFU_")
            .Build();

        return services.AddConfigProperties(merged);
    }

    private static IServiceCollection AddConfigProperties(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<SpicyTofuConfig>(
            configuration.GetSection("SpicyTofu"));

        services.Configure<PlaywrightConfig>(
            configuration.GetSection("Playwright"));

        services.Configure<TestExecution>(
            configuration.GetSection("TestExecution"));

        return services;
    }
}
