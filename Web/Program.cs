using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Web.Extensions;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddWebExtensions(context.Configuration);
    })
    .Build();