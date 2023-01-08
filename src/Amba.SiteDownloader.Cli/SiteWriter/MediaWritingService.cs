using Serilog;

namespace Amba.SiteDownloader.Cli.SiteWriter;

public class MediaWritingService
{
    private readonly string _outputDirectory;

    public MediaWritingService(string outputDirectory)
    {
        _outputDirectory = outputDirectory;
        if (!Directory.Exists(_outputDirectory))
        {
            Directory.CreateDirectory(_outputDirectory);
        }
    }

    public async Task<SaveResult> SaveMediaStream(Stream stream, string? contentTypeMediaType, string linkPath)
    {
        linkPath = linkPath.Trim('/');
        var fileName = GetMediaFileName(linkPath);
        var relativePath = Path.GetDirectoryName(linkPath);
        var directoryPath = Path.Combine(_outputDirectory, relativePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var destinationPath = Path.Combine(directoryPath, fileName);
        Log.Information("Saving {LinkPath}", linkPath);
        await using (StreamWriter streamWriter = new StreamWriter(destinationPath))
        {
            await stream.CopyToAsync(streamWriter.BaseStream);
        }

        return new SaveResult { LinkPath = linkPath, FilePath = destinationPath };
    }

    private string GetMediaFileName(string linkPath)
    {
        if (linkPath == null)
        {
            throw new ArgumentNullException(nameof(linkPath));
        }

        if (linkPath.Contains("?"))
        {
            linkPath = linkPath.Substring(0, linkPath.IndexOf("?"));
        }

        linkPath = linkPath.Trim();
        var fileName = Path.GetFileName(linkPath);
        return fileName;
    }

}
