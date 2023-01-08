using Amba.SiteDownloader.Cli.Common;
using Amba.SiteDownloader.Cli.Model;
using HtmlAgilityPack;
using Serilog;

namespace Amba.SiteDownloader.Cli.Processor;

public class LinkProcessor
{
    private readonly WebClient _webClient;
    private readonly SiteWriter _siteWriter;

    public LinkProcessor(WebClient webClient, SiteWriter siteWriter)
    {
        _webClient = webClient;
        _siteWriter = siteWriter;
    }

    public async Task<LinkProcessResult> Process(Link link, SiteDownloadContext context)
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
            await _siteWriter.SaveStream(stream, response.Content.Headers.ContentType.MediaType, link.Path);
            
            return new LinkProcessResult() { Error = false };
        }
        
        
        var html = await _webClient.DownloadPage(link.Path);
        Log.Information("Downloaded HTML: {url}", link.Path);
        
        await _siteWriter.SaveHtml(html, link.Path);
        var result = new LinkProcessResult();
        result.ChildLinks = ExtractLinks(html);
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
