using HtmlAgilityPack;
using Serilog;

namespace Amba.SiteDownloader.Cli;

public class SiteWriter
{
    private readonly string _outputDirectory;

    public SiteWriter(string outputDirectory)
    {
        _outputDirectory = outputDirectory;
        if (!Directory.Exists(_outputDirectory))
        {
            Directory.CreateDirectory(_outputDirectory);
        }
    }
    
 
    public async Task SaveStream(Stream stream, string? contentTypeMediaType, string linkPath)
    {
        Log.Information("Skip Saving {LinkPath}", linkPath);
    }

    public async Task SaveHtml(string html, string linkPath)
    {
        var relativeDirectory = GetDirectoryName(linkPath);
        var directory = Path.Combine(_outputDirectory, relativeDirectory);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        var fileName = GetHtmlFileName(linkPath);
        var destinationPath = Path.Combine(directory, fileName);
        await File.WriteAllTextAsync(destinationPath, html);
        Log.Information("Saving {LinkPath} to {DestinationPath}", linkPath, destinationPath);
    }

    private string GetHtmlFileName(string linkPath)
    {
        if (linkPath.Contains("?"))
        {
            var linkParts = linkPath.Split("?", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (linkParts.Length < 2)
            {
                return "index.html";
            }
            var result = linkParts[1].ToLower();
            if (!result.EndsWith(".html"))
            {
                result += ".html";
            }
            return result;
        }
        return "index.html";
    }

    private string GetDirectoryName(string linkPath)
    {
        var result = linkPath;
        if (linkPath.Contains("?"))
        {
            result = linkPath.Substring(0, linkPath.IndexOf("?", StringComparison.Ordinal));
        }
        result = result.Trim('/', ' ');        
        return result;

    }
}
