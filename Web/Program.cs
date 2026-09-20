using Web.Extension;
using Infrastructure.Extensions;

using Microsoft.Extensions.Hosting;


var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddWebExtensions(context.Configuration);
        services.AddInfrastructureDependencies(context.Configuration);
        services.AddWebAutomation(context.Configuration);
    })
    .Build();
