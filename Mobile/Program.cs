using Application.Runtime.RunService;

using Infrastructure.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Mobile.Extensions;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddMobileDependencies(context.Configuration);
        services.AddInfrastructureDependencies(context.Configuration);
        services.AddAutomation(context.Configuration);
    })
    .Build();

var runner = host.Services.GetRequiredService<IRunService>();

await runner.RunAsync();
