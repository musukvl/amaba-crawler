using System.Collections.Concurrent;
using System.Text.Json;

namespace Amba.SiteDownloader.Cli;

public class SavedLinksRepository
{
    private readonly string _historyFileName;
    public ConcurrentBag<SavedLink> SavedLinks { get; set; } = new ConcurrentBag<SavedLink>();

    public SavedLinksRepository(string historyFileName)
    {
        _historyFileName = historyFileName;
    }
    
    public void Add(SavedLink savedLink)
    {
        SavedLinks.Add(savedLink);
    }

    public async Task SaveHistory()
    {
        var json = JsonSerializer.Serialize(SavedLinks);
        await File.WriteAllTextAsync(_historyFileName, json);
    }
}
