using Amba.Crawler.Cli.Common;
using Amba.Crawler.Cli.Model;
using HtmlAgilityPack;
using Serilog;

namespace Amba.Crawler.Cli.Processor;

public class LinkProcessor
{
    private readonly WebClient _webClient;

    public LinkProcessor(WebClient webClient)
    {
        _webClient = webClient;

    }

    public async Task<LinkProcessResult> Process(Link link, SiteDownloadContext context)
    {
        var response = await _webClient.HttpClient.GetAsync(link.Path);
        if (!response.IsSuccessStatusCode)
        {
            return new LinkProcessResult() { Error = true, ErrorCode = response.StatusCode, ErrorMessage = response.ReasonPhrase };
        }

        if (response.Content.Headers.ContentType.MediaType != "text/html")
        {
            var stream = await response.Content.ReadAsStreamAsync();
            //TODO: save file
            Log.Information("Downloaded {contentType}: {url}", response.Content.Headers.ContentType.ToString(), link.Path);
            return new LinkProcessResult() { Error = false };
        }

        var html = await _webClient.DownloadPage(link.Path);
        Log.Information("Downloaded HTML: {url}", link.Path);

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
