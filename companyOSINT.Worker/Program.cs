using companyOSINT.Worker;
using companyOSINT.Worker.Detection;
using companyOSINT.Worker.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAI.Chat;

// Load .env file from solution root (if present) so dotnet run picks up env vars
var envFile = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".env");
if (File.Exists(envFile))
{
    foreach (var line in File.ReadAllLines(envFile))
    {
        var trimmed = line.Trim();
        if (trimmed.Length == 0 || trimmed.StartsWith('#'))
            continue;
        var sep = trimmed.IndexOf('=');
        if (sep <= 0)
            continue;
        var key = trimmed[..sep].Trim();
        var value = trimmed[(sep + 1)..].Trim();
        if (Environment.GetEnvironmentVariable(key) is null)
            Environment.SetEnvironmentVariable(key, value);
    }
}

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    // HTTP Clients
    services.AddHttpClient("Api", client =>
    {
        var baseUrl = context.Configuration["ApiBaseUrl"] ?? "https://www.company-osint.com";
        client.BaseAddress = new Uri(baseUrl);

        var apiKey = context.Configuration["ApiKey"]
                     ?? context.Configuration["API_KEY"]
                     ?? throw new InvalidOperationException("ApiKey is not configured");
        client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
    });

    services.AddHttpClient("Serper", client =>
    {
        client.BaseAddress = new Uri("https://google.serper.dev/");
        var apiKey = context.Configuration["SerperApiKey"]
                     ?? context.Configuration["SERPER_API_KEY"]
                     ?? throw new InvalidOperationException("SerperApiKey is not configured");
        client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
    });

    services.AddHttpClient("WebFetch", client =>
    {
        client.Timeout = TimeSpan.FromSeconds(15);
        client.MaxResponseContentBufferSize = 1_048_576; // 1 MB
        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36");
        client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8");
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("de-DE,de;q=0.9,en-US;q=0.8,en;q=0.7");
        client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");
        client.DefaultRequestHeaders.Add("Sec-CH-UA", "\"Chromium\";v=\"131\", \"Not_A Brand\";v=\"24\", \"Google Chrome\";v=\"131\"");
        client.DefaultRequestHeaders.Add("Sec-CH-UA-Mobile", "?0");
        client.DefaultRequestHeaders.Add("Sec-CH-UA-Platform", "\"Windows\"");
        client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
        client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
        client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "none");
        client.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
        client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
    }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        AutomaticDecompression = System.Net.DecompressionMethods.All
    });

    // Ollama via OpenAI SDK
    services.AddSingleton(_ =>
    {
        var ollamaUrl = context.Configuration["OllamaUrl"] ?? "http://192.168.12.117:11434/v1/";
        var ollamaModel = context.Configuration["OllamaModel"] ?? "gpt-oss:20b";

        var openAiClient = new OpenAI.OpenAIClient(new System.ClientModel.ApiKeyCredential("ollama"),
            new OpenAI.OpenAIClientOptions
            {
                Endpoint = new Uri(ollamaUrl),
                NetworkTimeout = TimeSpan.FromMinutes(5)
            });

        return openAiClient.GetChatClient(ollamaModel);
    });

    // Services
    services.AddSingleton<ICompanyApiClient, CompanyApiClient>();
    services.AddSingleton<ISerperSearchService, SerperSearchService>();
    services.AddSingleton<IWebScrapingService, WebScrapingService>();
    services.AddSingleton<IOllamaService, OllamaService>();
    services.AddSingleton<ICompanyMatchingService, CompanyMatchingService>();
    services.AddSingleton<ISectorCacheService, SectorCacheService>();
    services.AddSingleton<IWebsiteCheckService, WebsiteCheckService>();
    services.AddSingleton<IDetectionEngine, DetectionEngine>();
    services.AddSingleton<IConsentCheckService, ConsentCheckService>();

    // Workers
    services.AddHostedService<CompanyEnrichmentWorker>();
    services.AddHostedService<WebsiteEnrichmentWorker>();
});

var host = builder.Build();
await host.RunAsync();
