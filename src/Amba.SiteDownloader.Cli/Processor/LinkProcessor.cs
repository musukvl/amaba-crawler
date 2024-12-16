using Amba.SiteDownloader.Cli.Common;
using Amba.SiteDownloader.Cli.Model;
using Amba.SiteDownloader.Cli.SiteWriter;
using HtmlAgilityPack;
using Serilog;

namespace Amba.SiteDownloader.Cli.Processor;

public class LinkProcessor
{
    private readonly WebClient _webClient;
    private readonly MediaWritingService _mediaWritingService;
    private readonly HtmlWritingService _htmlWritingService;

    public LinkProcessor(WebClient webClient, MediaWritingService mediaWritingService, HtmlWritingService htmlWritingService)
    {
        _webClient = webClient;
        _mediaWritingService = mediaWritingService;
        _htmlWritingService = htmlWritingService;
    }

    public async Task<LinkProcessResult> Process(Link link)
    {
        var response = await _webClient.HttpClient.GetAsync(link.Path);
        if (!response.IsSuccessStatusCode)
        {
            return new LinkProcessResult() { Error = true, ErrorCode = response.StatusCode, ErrorMessage = response.ReasonPhrase };
        }

        var mediaType = response.Content.Headers.ContentType.MediaType;
        if (mediaType != "text/html")
        {
            await using var stream = await response.Content.ReadAsStreamAsync();
            
            Log.Information("Downloaded {contentType}: {url}", response.Content.Headers.ContentType.ToString(), link.Path);
            var saveMediaResult = await _mediaWritingService.SaveMediaStream(stream, response.Content.Headers.ContentType.MediaType, link.Path);
            
            return new LinkProcessResult { Error = false, SavedFilePath = saveMediaResult.FilePath };
        }
        
        var html = await _webClient.DownloadPage(link.Path);
        Log.Information("Downloaded HTML: {url}", link.Path);

        var saveHtmlResult = await _htmlWritingService.SaveHtml(html, link.Path);
        var result = new LinkProcessResult
        {
            SavedFilePath = saveHtmlResult.FilePath,
            ChildLinks = ExtractLinks(html)
        };
        return result;
    }
    
    private IEnumerable<Link> ExtractLinks(string html)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(html);
        doc.OptionEmptyCollection = true;
        
        var links = new List<Link>();
        // extract pages
        foreach (var aNode in doc.DocumentNode.SelectNodes("//a"))
        {
            var href = aNode.GetAttributeValue("href", "");
            href = RemoveAnchor(href);
            if (string.IsNullOrEmpty(href))
                continue;

            if (href.StartsWith("/"))
            {
                links.Add(new Link() { Path = href, Type = LinkType.LocalPage });
            }
        }

        // extract images
        foreach (var aNode in doc.DocumentNode.SelectNodes("//img"))
        {
            var href = aNode.GetAttributeValue("src", "");
            href = RemoveAnchor(href);

            if (string.IsNullOrEmpty(href))
                continue;
            if (href.StartsWith("/"))
            {
                links.Add(new Link() { Path = href, Type = LinkType.LocalImage });
            }
        }

        foreach (var aNode in doc.DocumentNode.SelectNodes("//script"))
        {
            var src = aNode.GetAttributeValue("src", "");
            if (string.IsNullOrEmpty(src))
                continue;
            if (src.StartsWith("/"))
            {
                links.Add(new Link() { Path = src, Type = LinkType.LocalScript });
            }
        } 
        foreach (var aNode in doc.DocumentNode.SelectNodes("//link"))
        {
            var src = aNode.GetAttributeValue("href", "");
            if (string.IsNullOrEmpty(src))
                continue;
            if (src.StartsWith("/"))
            {
                links.Add(new Link() { Path = src, Type = LinkType.LocalStyle });
            }
        }
        
        return links;
    }

    private string RemoveAnchor(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return url;
        var index = url.IndexOf('#');
        if (index == -1)
            return url;
        return url.Substring(0, index);
    }
}
