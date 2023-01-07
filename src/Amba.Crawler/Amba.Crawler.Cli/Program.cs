using Amba.Crawler.Cli;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Amba.Crawler.Client;

class Program
{
    [Option(Description = "The URL to crawl", Template = "-u|--url")]
    public string Url { get; }
    
    [Option(Description = "Output directory", Template = "-o|--output")]
    public string Output { get; }
    
    [Option(Description = "History file", Template = "-h|--history-file")]
    public string HistoryFile { get; } = "history.txt";

    private void OnExecute()
    {
        var result = _service.Crawl(Url);
        Console.WriteLine(result);
    }
    
    
    static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();            
        
        Log.Information("Start initializing...");
        
        var services = new ServiceCollection();
        ConfigureServices(services); 

        var serviceProvider = services.BuildServiceProvider();

        var app = new CommandLineApplication<Program>();
        app.Conventions
            .UseDefaultConventions()
            .UseConstructorInjection(serviceProvider);

        app.Execute(args);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient<PageDownloader>(clientConfig =>
        { 
            clientConfig.BaseAddress = new Uri("http://localhost:5000");
        });
        services.AddSingleton<CrawlService>();
    }

    private readonly CrawlService _service;

    public Program(CrawlService service)
    {
        _service = service;
    }
}


public class PageDownloader
{

    private readonly HttpClient _client;

    public PageDownloader(HttpClient client)
    {
        _client = client;
    }

    public async Task<string> DownloadPage(string url)
    {
        var response = await _client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
 
