using Amba.SiteDownloader.Cli.Common;
using Amba.SiteDownloader.Cli.Model;
using Amba.SiteDownloader.Cli.Processor;
using Amba.SiteDownloader.Cli.SiteWriter;
using Serilog;

namespace Amba.SiteDownloader.Cli;

public class SiteDownloadManager
{
    private readonly LinkProcessor _linkProcessor;

    public SiteDownloadManager(WebClient webClient, LinkProcessor linkProcessor)
    {
        _linkProcessor = linkProcessor; 
    } 
    
    public async Task DownloadSite(string startUrl, int maxDepth = 3)
    {
        var uri = new Uri(startUrl);
        var root = new Link{Path = uri.PathAndQuery};
        
        var context = new SiteDownloadContext();
        context.Enqueue(new []{root} );

        do
        {
            if (context.ParsingQueue.TryDequeue(out var link))
            {
                var processResult = await _linkProcessor.Process(link);
                if (processResult.ChildLinks.Any())
                {
                    context.Enqueue(processResult.ChildLinks);
                }
            }
        } while (!context.ParsingQueue.IsEmpty);
        
        Log.Information("Crawling {url}", startUrl);
    }
}
