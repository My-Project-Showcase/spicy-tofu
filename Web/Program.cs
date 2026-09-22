using Web.Extension;

using Application.Runtime.RunService;

using Infrastructure.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddWebExtensions(context.Configuration);
        services.AddInfrastructureDependencies(context.Configuration);
        services.AddWebAutomation(context.Configuration);
    })
    .Build();

var runner = host.Services.GetRequiredService<IRunService>();

await runner.RunAsync();
