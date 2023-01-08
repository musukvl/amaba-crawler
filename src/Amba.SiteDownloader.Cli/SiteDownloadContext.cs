using Amba.SiteDownloader.Cli.Model;
using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace Amba.SiteDownloader.Cli;

public class SiteDownloadContext
{
    public ConcurrentQueue<Link> ParsingQueue { get; set; } = new();
    public ConcurrentBag<string> VisitedLinks { get; set; } = new ConcurrentBag<string>();

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

public class SavedLink
{
    public string Link { get; set; }
    public string FilePath { get; set; }
    
}
