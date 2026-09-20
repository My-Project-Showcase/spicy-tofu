using Infrastructure.Extensions;
using Microsoft.Extensions.Hosting;
using Mobile.Extensions;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddMobileDependencies(context.Configuration);
        services.AddInfrastructureDependencies(context.Configuration);
        services.AddMobileAutomation(context.Configuration);
    })
    .Build();
