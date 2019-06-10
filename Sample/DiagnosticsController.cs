using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Sample
{
    public class DiagnosticsController : Controller
    {
        private readonly HttpClient _httpClient;

        public DiagnosticsController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("AkamaiAuth");
        }

        [HttpGet("/diagnostics")]
        public async Task<IActionResult> Get()
        {
            var response = await _httpClient.GetAsync("diagnostic-tools/v2/ghost-locations/available");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsAsync<JObject>();
            return Json(content);
        }
    }
}