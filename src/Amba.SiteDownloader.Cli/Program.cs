using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Amba.SiteDownloader.Cli;

public static class Program
{

    static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();            
        
        Log.Information("Start initializing...");
        
        var services = new ServiceCollection();
        
        var serviceProvider = services.BuildServiceProvider();

        var app = new CommandLineApplication<DownloadSiteCommand>();
        app.Conventions
            .UseDefaultConventions()
            .UseConstructorInjection(serviceProvider);

        app.Execute(args);
    }
}
