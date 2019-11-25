using System;
using Newtonsoft.Json;
using OpenQA.Selenium;

namespace ZammoTts
{
    internal class MyCookie
    {
        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("expiry")]
        public long Expiry { get; set; }

        [JsonProperty("httpOnly")]
        public bool IsHttpOnly { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("secure")]
        public bool Secure { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        public DateTime GetExpiryDateTime()
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(Expiry).ToLocalTime();

            return dtDateTime;
        }

        // public Cookie (String name, String value, String domain, String path, Nullable<DateTime> expiry);
        public Cookie GetCookie()
        {
            return new Cookie(Name, Value, Domain, Path, GetExpiryDateTime());
        }
    }
}