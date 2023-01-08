using Amba.SiteDownloader.Cli.Model;
using System.Net;

namespace Amba.SiteDownloader.Cli.Processor;

public class LinkProcessResult
{
    public IEnumerable<Link> ChildLinks { get; set; } = new List<Link>();
    public bool Error { get; set; }
    public HttpStatusCode ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string SavedFilePath { get; set; }
}
