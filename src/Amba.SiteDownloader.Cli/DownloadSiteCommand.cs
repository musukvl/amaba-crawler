using Amba.SiteDownloader.Cli.Common;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;

namespace Amba.SiteDownloader.Cli;

class DownloadSiteCommand
{
    [Option(Description = "The URL to crawl", Template = "-u|--url")]
    public string Url { get; }
    
    [Option(Description = "Output directory", Template = "-o|--output")]
    public string Output { get; }
    
    [Option(Description = "History file", Template = "-h|--history-file")]
    public string HistoryFile { get; } = "history.txt";
    
    private async Task<int> OnExecute()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        var provider = services.BuildServiceProvider();

        await using var scope = provider.CreateAsyncScope(); 
        var crawlService = scope.ServiceProvider.GetService<SiteDownloadManager>();
        
        await crawlService.DownloadSite(Url);
        return 0;
    }
    
    private void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient<WebClient>(clientConfig =>
        { 
            var uri = new Uri(Url);
            clientConfig.BaseAddress = new Uri(uri.AbsoluteUri);
        });
        services.AddSingleton<SiteDownloadManager>();
        services.AddSingleton<SiteWriter>(new SiteWriter(Output));
    }
}
