using Domain.Runtime.Environment.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mobile.Extensions;

public static class MobileExtensions
{

    public static IServiceCollection AddMobileDependencies(
        this IServiceCollection service,
        IConfiguration configuration
    ){

        service.AddConfigProperties(configuration);
        service.AddEnvCompatibility(configuration);

        return service;
    }

    private static void AddConfigProperties(
        this IServiceCollection service,
        IConfiguration configuration)
    {
        service.Configure<SpicyTofuConfig>(
            configuration.GetSection("SpicyTofu"));

        service.Configure<TestExecution>(
            configuration.GetSection("TestExecution"));

        service.Configure<Appium>(
            configuration.GetSection("Appium"));
    }

    private static void AddEnvCompatibility(
        this IServiceCollection service,
        IConfiguration configuration)
    {
        var merged = new ConfigurationBuilder()
            .AddConfiguration(configuration)
            .AddEnvironmentVariables("TOFU_")
            .Build();

        service.AddConfigProperties(merged);
    }
}
