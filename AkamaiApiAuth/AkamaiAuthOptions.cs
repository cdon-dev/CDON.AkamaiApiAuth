using System.Collections.Generic;

namespace AkamaiApiAuth
{
    public class AkamaiAuthOptions
    {
        public string ClientToken { get; set; }
        public string AccessToken { get; set; }
        public string ClientSecret { get; set; }
        public string RequestContentType { get; set; } = "application/json";
        public IList<string> IncludeHeaders { get; set; }
        public int? MaxBodyHashSize { get; set; }
    }
}