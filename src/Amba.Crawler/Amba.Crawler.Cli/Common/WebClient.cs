namespace Amba.Crawler.Cli.Common;

public class WebClient
{

    private readonly HttpClient _client;

    public WebClient(HttpClient client)
    {
        _client = client;
    }

    public HttpClient HttpClient => _client;
    
    public async Task<string> DownloadPage(string url)
    {
        var response = await _client.GetAsync(url);
        
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
