using Microsoft.Extensions.Hosting;
using Mobile.Extensions;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>

    {
        services.AddMobileDependencies(context.Configuration);
    });
