namespace Amba.SiteDownloader.Cli.Model;

public class Link
{
    public string Path { get; set; }
    public int Depth { get; set; }
    public LinkType Type { get; set; }
}
