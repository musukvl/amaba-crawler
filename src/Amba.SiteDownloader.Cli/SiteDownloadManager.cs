using Amba.SiteDownloader.Cli.Common;
using Amba.SiteDownloader.Cli.Model;
using Amba.SiteDownloader.Cli.Processor;
using Serilog;

namespace Amba.SiteDownloader.Cli;

public class SiteDownloadManager
{
    private readonly WebClient _webClient;
    private readonly SiteWriter _siteWriter;

    public SiteDownloadManager(WebClient webClient, SiteWriter siteWriter)
    {
        _webClient = webClient;
        _siteWriter = siteWriter;
    } 
    
    public async Task DownloadSite(string startUrl, int maxDepth = 3)
    {
        var linkProcessor = new LinkProcessor(_webClient, _siteWriter);
        var uri = new Uri(startUrl);
        var root = new Link{Path = uri.PathAndQuery};
        
        var context = new SiteDownloadContext();
        context.Enqueue(new []{root} );

        do
        {
            if (context.ParsingQueue.TryDequeue(out var link))
            {
                var processResult = await linkProcessor.Process(link, context);
                context.Enqueue(processResult.ChildLinks);
            }
        } while (!context.ParsingQueue.IsEmpty);
        
        Log.Information("Crawling {url}", startUrl);
    }
}
