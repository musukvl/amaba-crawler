using Serilog;

namespace Amba.SiteDownloader.Cli.SiteWriter;

public class HtmlWritingService
{

    private readonly string _outputDirectory;

    public HtmlWritingService(string outputDirectory)
    {
        _outputDirectory = outputDirectory;
        if (!Directory.Exists(_outputDirectory))
        {
            Directory.CreateDirectory(_outputDirectory);
        }
    }

    public async Task<SaveResult> SaveHtml(string html, string linkPath)
    {
        var relativeDirectory = GetHtmlDestinationDirectoryName(linkPath);
        var directory = Path.Combine(_outputDirectory, relativeDirectory);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var fileName = GetHtmlFileName(linkPath);
        var destinationPath = Path.Combine(directory, fileName);
        await File.WriteAllTextAsync(destinationPath, html);
        Log.Information("Saving {LinkPath} to {DestinationPath}", linkPath, destinationPath);
        return new SaveResult { LinkPath = linkPath, FilePath = destinationPath };
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

    private string GetHtmlDestinationDirectoryName(string linkPath)
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
