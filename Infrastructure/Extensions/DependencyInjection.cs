using Application.Automation.Web;
using Infrastructure.Automation.Web;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureDependencies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
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
}