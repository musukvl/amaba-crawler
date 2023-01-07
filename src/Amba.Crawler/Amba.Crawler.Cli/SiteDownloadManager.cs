using Amba.Crawler.Cli.Common;
using Amba.Crawler.Cli.Model;
using Amba.Crawler.Cli.Processor;
using Serilog;

namespace Amba.Crawler.Cli;

public class SiteDownloadManager
{
    private readonly WebClient _webClient;

    public SiteDownloadManager(WebClient webClient)
    {
        _webClient = webClient; 
    } 
    
    public async Task DownloadSite(string startUrl, int maxDepth = 3)
    {
        var uri = new Uri(startUrl);
        var absoluteUri = uri.AbsoluteUri;
        
        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(absoluteUri);
        
        var linkProcessor = new LinkProcessor(_webClient);
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
