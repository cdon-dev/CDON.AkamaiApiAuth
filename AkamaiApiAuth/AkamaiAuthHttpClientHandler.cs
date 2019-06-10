using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

namespace AkamaiApiAuth
{
    public class AkamaiAuthHttpClientHandler : DelegatingHandler
    {
        private readonly AkamaiAuthOptions _options;
        private readonly AkamaiAuthGenerator _akamaiAuthGenerator;

        public AkamaiAuthHttpClientHandler(AkamaiAuthOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _akamaiAuthGenerator = new AkamaiAuthGenerator(options);
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Content != null && !string.IsNullOrEmpty(_options.RequestContentType))
            {
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(_options.RequestContentType);
            }

            await SignRequest(request);
            var response = await base.SendAsync(request, cancellationToken);
            ValidateResponse(response);

            return response;
        }

        private async Task SignRequest(HttpRequestMessage request)
        {
            request.Headers.Remove(HeaderNames.Authorization);
            var authHeader = await _akamaiAuthGenerator.Generate(request);
            request.Headers.Add(HeaderNames.Authorization, authHeader);
        }

        private static void ValidateResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            if (!response.Headers.TryGetValues(HeaderNames.Date, out var values))
            {
                return;
            }

            var value = values.FirstOrDefault();
            if (value == null || DateTime.TryParse(value, out var date))
            {
                return;
            }

            var diff = (DateTime.UtcNow - date).TotalSeconds;
            if (Math.Abs(diff) > 30)
            {
                throw new Exception("Local server date is more than 30s out of sync with remote server");
            }
        }
    }
}