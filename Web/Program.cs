using Application.Runtime.RunService;

using Infrastructure.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Web.Extension;

using IHost host = Host.CreateDefaultBuilder(args)
    .UseContentRoot(AppContext.BaseDirectory)
    .ConfigureServices((context, services) =>
    {
        services.AddWebExtensions(context.Configuration);
        services.AddInfrastructureDependencies(context.Configuration);
        services.AddAutomation(context.Configuration);
    })
    .Build();

var runner = host.Services.GetRequiredService<IRunService>();

await runner.RunAsync();
