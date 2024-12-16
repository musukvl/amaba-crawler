namespace Amba.SiteDownloader.Cli.Model;

public class Link
{
    public required string Path { get; init; }
    public int Depth { get; set; }
    public LinkType Type { get; set; }
}
