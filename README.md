# CDON.AkamaiApiAuth

An authentication handler for `HttpClient`.

Vanilla example:

```csharp
var options = new AkamaiAuthOptions
{
    ClientSecret = "CLIENTSECRET",
    ClientToken = "CLIENTTOKEN",
    AccessToken = "ACCESSTOKEN"
};
var handler = new AkamaiAuthHttpClientHandler(options, new HttpClientHandler());
var client = new HttpClient(handler);
var response = await client.GetAsync("https://akzz-XXXXXXXXXXXXXXXX-XXXXXXXXXXXXXXXX.luna.akamaiapis.net/diagnostic-tools/v2/ghost-locations/available");
var result = await response.Content.ReadAsStringAsync();
```

Example for ASP.NET Core:

```csharp
// Configuration in UserSecrets, appsettings.json or similar
{
  "AkamaiAuth": {
    "ClientSecret": "CLIENTSECRET",
    "ClientToken": "CLIENTTOKEN",
    "AccessToken": "ACCESSTOKEN"
  },
  "AkamaiApiUrl": "https://akzz-XXXXXXXXXXXXXXXX-XXXXXXXXXXXXXXXX.luna.akamaiapis.net/"
}

// ConfigureServices in Startup
services.Configure<AkamaiAuthOptions>(_configuration.GetSection("AkamaiAuth"));
services
    .AddHttpClient(
        "AkamaiAuth", client => client.BaseAddress = _configuration.GetValue<Uri>("AkamaiApiUrl"))
    .AddHttpMessageHandler(sp => new AkamaiAuthHttpClientHandler(
        sp.GetService<IOptions<AkamaiAuthOptions>>().Value));

// Code calling API
public class DiagService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DiagService(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public async Task<string> GetDiag()
    {
        var client = _httpClientFactory.CreateClient("AkamaiAuth");
        var response = await client.GetAsync("diagnostic-tools/v2/ghost-locations/available");
        return await response.Content.ReadAsStringAsync();
    }
}
```
