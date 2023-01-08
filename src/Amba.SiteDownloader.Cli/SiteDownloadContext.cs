using Amba.SiteDownloader.Cli.Model;
using System.Collections.Concurrent;

namespace Amba.SiteDownloader.Cli;

public class SiteDownloadContext
{
    public ConcurrentQueue<Link> ParsingQueue { get; set; } = new();
    public ConcurrentBag<string> VisitedLinks { get; set; } = new ConcurrentBag<string>();
    public ConcurrentBag<string> ParsedLinks { get; set; } = new ConcurrentBag<string>();

    public void Enqueue(IEnumerable<Link> parseResultLinks)
    {
        foreach (var link in parseResultLinks)
        {
            if (!VisitedLinks.Contains(link.Path))
            {
                ParsingQueue.Enqueue(link);
                VisitedLinks.Add(link.Path);
            }
        }
    }
}
