using System;
using Newtonsoft.Json;

namespace Emzi0767.OwoDotNet
{
    /// <summary>
    /// Represents uploader's configuration.
    /// </summary>
    public struct OwoConfiguration
    {
        /// <summary>
        /// Gets the API key for uploading to whats-th.is
        /// </summary>
        [JsonProperty("api_key")]
        public string ApiKey { get; internal set; }

        /// <summary>
        /// Gets the preferred uri for uploaded files.
        /// </summary>
        [JsonProperty("preferred_url")]
        public Uri PreferredUri { get; internal set; }

        /// <summary>
        /// Gets the default API base URL.
        /// </summary>
        [JsonProperty("api_base_url")]
        public Uri ApiBaseUri { get; internal set; }

        /// <summary>
        /// Creates a default whats-th.is configuration.
        /// </summary>
        /// <returns>Default initialized configuration.</returns>
        public static OwoConfiguration CreateDefault()
        {
            return new OwoConfiguration
            {
                ApiKey = "your-api-key-here",
                PreferredUri = new Uri("https://owo.whats-th.is"),
                ApiBaseUri = new Uri("https://api.awau.moe"),
            };
        }
    }
}
