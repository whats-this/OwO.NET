// This file is part of OwO.NET project
//
// Copyright 2017-2018 Emzi0767
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Net;
using Newtonsoft.Json;

namespace Emzi0767.OwoDotNet
{
    /// <summary>
    /// Represents uploader's configuration.
    /// </summary>
    public class OwoConfiguration
    {
        /// <summary>
        /// Returns the default uploader configuration.
        /// </summary>
        public static OwoConfiguration Default { get; } = new OwoConfiguration
        {
            ApiKey = "your-api-key-here"
        };

        /// <summary>
        /// Gets or sets the API key for uploading to whats-th.is.
        /// </summary>
        [JsonProperty("api_key")]
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the preferred uri for uploaded files.
        /// </summary>
        [JsonProperty("upload_url")]
        public Uri UploadUrl { get; set; } = new Uri("https://owo.whats-th.is");

        /// <summary>
        /// Gets or sets the preferred uri for url shortener links.
        /// </summary>
        [JsonProperty("shortener_url")]
        public Uri ShortenerUrl { get; set; } = new Uri("https://awau.moe");

        /// <summary>
        /// Gets or sets the default API base URL.
        /// </summary>
        [JsonProperty("api_base_url")]
        public Uri ApiBaseUri { get; set; } = new Uri("https://api.awau.moe");

        /// <summary>
        /// Gets or sets the proxy settings for this client.
        /// </summary>
        [JsonProperty("proxy")]
        public ProxyConfiguration ProxySettings { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="OwoConfiguration"/> with null values.
        /// </summary>
        public OwoConfiguration() { }

        internal OwoConfiguration(OwoConfiguration other)
        {
            this.ApiKey = other.ApiKey;
            this.UploadUrl = other.UploadUrl;
            this.ShortenerUrl = other.ShortenerUrl;
            this.ApiBaseUri = other.ApiBaseUri;
            this.ProxySettings = new ProxyConfiguration(other.ProxySettings);
        }
    }

    /// <summary>
    /// Represents proxy configuration for whats-th.is uploader. These settings will be used when making HTTP connections to the service.
    /// </summary>
    public class ProxyConfiguration
    {
        /// <summary>
        /// Gets or sets the full address of the proxy server.
        /// </summary>
        [JsonProperty("address")]
        public Uri Address { get; set; }

        /// <summary>
        /// Gets or sets the username to use for the proxy.
        /// </summary>
        [JsonProperty("username")]
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password to use for the proxy.
        /// </summary>
        [JsonProperty("password")]
        public string Password { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="ProxyConfiguration"/> with null values.
        /// </summary>
        public ProxyConfiguration() { }

        internal ProxyConfiguration(ProxyConfiguration other)
        {
            this.Address = other.Address;
            this.Username = other.Username;
            this.Password = other.Password;
        }

        internal IWebProxy CreateProxy()
#if NETSTANDARD1_1
            => throw new PlatformNotSupportedException("Proxy settings are not supported on this platform.");
#else
            => new WebProxy(this.Address, false, new string[0], new NetworkCredential(this.Username, this.Password));
#endif
    }
}
