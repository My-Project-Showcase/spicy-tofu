using Application.Automation.Mobile;
using Application.Automation.Web;
using Application.Logging;
using Application.Runtime.JsonService;
using Application.Runtime.RunService;

using Domain.Runtime.Environment.Configuration;

using Infrastructure.Automation.Mobile;
using Infrastructure.Automation.Web;
using Infrastructure.Logging;
using Infrastructure.Runtime.JsonService;
using Infrastructure.Runtime.RunServices;
using Infrastructure.Runtime.TestExecution;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureDependencies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddServices(configuration);
        return services;
    }

    public static IServiceCollection AddWebAutomation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (!string.Equals(configuration["SpicyTofu:Platform"], "Web", StringComparison.OrdinalIgnoreCase))
        {
            return services;
        }

        services.AddSingleton<BrowserHost>();
        services.AddScoped<IWebDriver, WebDriver>();

        return services;
    }

    public static IServiceCollection AddMobileAutomation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (!string.Equals(configuration["SpicyTofu:Platform"], "Mobile", StringComparison.OrdinalIgnoreCase))
        {
            return services;
        }

        services.AddSingleton<MobileHost>();
        services.AddScoped<IMobileDriver, MobileDriver>();

        return services;
    }

    public static IServiceCollection AddServices(
        this IServiceCollection service,
        IConfiguration configuration)
    {
        service.Configure<LoggingConfig>(configuration.GetSection("Logging"));

        service.AddSingleton<ILogger, Logger>();
        service.AddSingleton<IPrintStrategy, ConsolePrintStrategy>();

        service.AddSingleton<IJsonService, JsonService>();
        service.AddSingleton<TestsLoadedHandler>();
        service.AddSingleton<IRunService, RunService>();
        return service;
    }
}
