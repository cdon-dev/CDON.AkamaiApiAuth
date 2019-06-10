using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CDON.AkamaiApiAuth
{
    public class AkamaiAuthGenerator
    {
        private readonly AkamaiAuthOptions _options;
        private readonly SignType _signVersion;
        private readonly HashType _hashVersion;

        public AkamaiAuthGenerator(AkamaiAuthOptions options)
        {
            ValidateOptions(options);

            _options = options;
            _signVersion = SignType.HMACSHA256;
            _hashVersion = HashType.SHA256;
        }

        public async Task<string> Generate(HttpRequestMessage request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var timestamp = DateTime.UtcNow.ToIso8601();
            var requestData = await GetRequestData(request);
            var authData = GetAuthDataValue(timestamp);
            return GetAuthorizationHeaderValue(timestamp, authData, requestData);
        }

        private async Task<string> GetRequestData(HttpRequestMessage request)
        {
            var bodyStream = request.Content != null
                ? await request.Content.ReadAsStreamAsync()
                : null;

            var headers = GetRequestHeaders(request.Headers);
            var bodyHash = !request.Method.Equals(HttpMethod.Get)
                ? GetRequestStreamHash(bodyStream)
                : string.Empty;

            return
                $"{request.Method.Method.ToUpperInvariant()}\t{request.RequestUri.Scheme}\t" +
                $"{request.RequestUri.Host}\t{request.RequestUri.PathAndQuery}\t{headers}\t{bodyHash}\t";
        }

        private string GetRequestHeaders(HttpHeaders requestHeaders)
        {
            if (_options.IncludeHeaders == null)
            {
                return string.Empty;
            }

            return string.Concat(
                from name in _options.IncludeHeaders
                let values = requestHeaders.GetValues(name)
                where values.Any()
                let value = string.Concat(values)
                let cleanedValue = Regex.Replace(value.Trim(), @"\s+", " ", RegexOptions.Compiled)
                select $"{name}:{cleanedValue}");
        }

        private string GetAuthDataValue(string timestamp)
        {
            var nonce = Guid.NewGuid();
            return
                $"{_signVersion.Name} client_token={_options.ClientToken};" +
                $"access_token={_options.AccessToken};" +
                $"timestamp={timestamp};" +
                $"nonce={nonce.ToString().ToLowerInvariant()};";
        }

        private string GetRequestStreamHash(Stream requestStream)
        {
            if (requestStream == null)
            {
                return string.Empty;
            }

            if (!requestStream.CanRead) throw new IOException("Cannot read stream to compute hash");
            if (!requestStream.CanSeek) throw new IOException("Stream must be seekable!");

            var streamHash = requestStream.ComputeHash(
                _hashVersion.ChecksumAlgorithm, _options.MaxBodyHashSize).ToBase64();
            requestStream.Seek(0, SeekOrigin.Begin);
            return streamHash;
        }

        private string GetAuthorizationHeaderValue(
            string timestamp, string authData, string requestData)
        {
            var signingKey = timestamp.ToByteArray()
                .ComputeKeyedHash(_options.ClientSecret, _signVersion.Algorithm).ToBase64();
            var authSignature = $"{requestData}{authData}".ToByteArray()
                .ComputeKeyedHash(signingKey, _signVersion.Algorithm).ToBase64();
            return $"{authData}signature={authSignature}";
        }

        private static void ValidateOptions(AkamaiAuthOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrEmpty(options.ClientToken))
            {
                throw new ArgumentException("options.ClientToken is not set", nameof(options));
            }

            if (string.IsNullOrEmpty(options.AccessToken))
            {
                throw new ArgumentException("options.AccessToken is not set", nameof(options));
            }

            if (string.IsNullOrEmpty(options.ClientSecret))
            {
                throw new ArgumentException("options.ClientSecret is not set", nameof(options));
            }
        }

        private class SignType
        {
            public static readonly SignType HMACSHA256 = new SignType("EG1-HMAC-SHA256", "HMACSHA256");

            private SignType(string name, string algorithm)
            {
                Name = name;
                Algorithm = algorithm;
            }

            public string Name { get; }
            public string Algorithm { get; }
        }

        private class HashType
        {
            public static readonly HashType SHA256 = new HashType("SHA256");

            private HashType(string checksumAlgorithm)
            {
                ChecksumAlgorithm = checksumAlgorithm;
            }

            public string ChecksumAlgorithm { get; }
        }
    }
}