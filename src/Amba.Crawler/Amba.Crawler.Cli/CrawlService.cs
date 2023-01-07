using HtmlAgilityPack;
using Serilog;
using System.Collections.Concurrent;

namespace Amba.Crawler.Cli;

public class ParsingContext
{
    public ConcurrentQueue<Link> ParsingQueue { get; set; } = new();
    public ConcurrentBag<string> VisitedLinks { get; set; } = new ConcurrentBag<string>();
    public ConcurrentBag<string> ParsedLinks { get; set; } = new ConcurrentBag<string>();

}

public class CrawlService
{
    public bool Crawl(string startUrl, int maxDepth = 3)
    {
        var context = new ParsingContext();
        
        var uri = new Uri(startUrl);
        var absoluteUri = uri.AbsoluteUri;
        
        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(absoluteUri);
        
        var downloader = new Downloader(httpClient);
        var root = new Link{Path = uri.PathAndQuery};
        var parseResult = downloader.Download(root, context);
        
        
        
        
        Log.Information("Crawling {url}", startUrl);
        return true;
    }
}



public class Downloader
{
    private readonly HttpClient _httpClient;

    public Downloader(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public ParseResult Download(Link link, ParsingContext context)
    {
        
        var html = _httpClient.GetStringAsync(link.Path).Result;

        HtmlDocument doc = new();
        doc.LoadHtml(html);    
        
        doc.OptionEmptyCollection = true;

        var result = new ParseResult();
        
        foreach (var aNode in doc.DocumentNode.SelectNodes("//a"))
        {
            var href = aNode.GetAttributeValue("href", "");
            
            if (string.IsNullOrEmpty(href))
                continue;

            if (href.StartsWith("/"))
            {
                result.Links.Add(new Link(){Path = href});
            } 
            
            Log.Information("Found link {href}", href);    
        }
        
        Log.Information("Processing {url}", link.Path);
        return new ParseResult();
    } 
}

public class ParseResult
{
    public List<Link> Links { get; set; } = new List<Link>();
}

public class Link
{
    public string Path { get; set; }
    public int Depth { get; set; }
}
