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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Emzi0767.OwoDotNet
{
    /// <summary>
    /// Represents an uploader interface for whats-th.is
    /// </summary>
    public class OwoClient : IDisposable
    {
        private const string POMF_UPLOAD = "/upload/pomf";
        private const string POLR_SHORTEN = "/shorten/polr";
        private const string DOMAIN_LIST = "/public-cdn-domains.txt";
        private static Encoding Encoding { get; } = new UTF8Encoding(false);

        /// <summary>
        /// Gets or sets the <see cref="System.Net.Http.HttpClient"/> instance to use for this uploader.
        /// </summary>
        protected HttpClient HttpClient { get; }

        /// <summary>
        /// Gets or sets the API key to use for uploading.
        /// </summary>
        protected string ApiKey { get; }

        /// <summary>
        /// Gets or sets the base API uri.
        /// </summary>
        protected Uri ApiBaseUri { get; }

        /// <summary>
        /// Gets or sets the upload endpoint.
        /// </summary>
        protected Uri UploadEndpoint { get; }

        /// <summary>
        /// Gets or sets the shorten endpoint.
        /// </summary>
        protected Uri ShortenEndpoint { get; }

        /// <summary>
        /// Gets or sets the client's configuration.
        /// </summary>
        protected OwoConfiguration Configuration { get; }

        private bool _disposed = false;

        /// <summary>
        /// Creates a new <see cref="OwoClient"/>.
        /// </summary>
        /// <param name="api_key">API key to use for this uploader.</param>
        public OwoClient(OwoConfiguration config)
        {
            var a = typeof(OwoClient)
                .GetTypeInfo()
                .Assembly;
            var v = a.GetName().Version;

            this.Configuration = new OwoConfiguration(config);
            this.ApiKey = this.Configuration.ApiKey;
            this.ApiBaseUri = this.Configuration.ApiBaseUri;

            // create the upload URI
            var ub = new UriBuilder(this.ApiBaseUri)
            {
                Path = POMF_UPLOAD
            };
            this.UploadEndpoint = ub.Uri;

            // create the shorten URI
            ub = new UriBuilder(this.ApiBaseUri)
            {
                Path = POLR_SHORTEN
            };
            this.ShortenEndpoint = ub.Uri;

            // create proxy setting handler
            var httphandler = new HttpClientHandler
            {
                UseProxy = this.Configuration.ProxySettings != null,
                Proxy = this.Configuration.ProxySettings?.CreateProxy()
            };

            this.HttpClient = new HttpClient(httphandler)
            {
                BaseAddress = this.ApiBaseUri
            };

            this.HttpClient.DefaultRequestHeaders.Add("User-Agent", string.Concat("WhatsThisClient (https://github.com/whats-this/OwO.NET, v", v.ToString(3), ")"));
        }

        /// <summary>
        /// Asynchronously shortens an URL using whats-th.is
        /// </summary>
        /// <param name="url">Url to shorten.</param>
        /// <returns>Shortened URL.</returns>
        public async Task<Uri> ShortenUrlAsync(Uri url)
        {
            var get_args = new Dictionary<string, string>
            {
                ["key"] = this.ApiKey,
                ["action"] = "shorten",
                ["url"] = url.ToString()
            };

            var turl = string.Concat(this.ShortenEndpoint, "?", get_args.ToQueryString());

            var req = new HttpRequestMessage(HttpMethod.Get, new Uri(turl));

            var res = await this.HttpClient.SendAsync(req);

            var buff = await res.Content.ReadAsByteArrayAsync();
            var dat = Encoding.GetString(buff, 0, buff.Length);

            return this.MakeShortenedUri(dat);
        }

        /// <summary>
        /// Asynchronously uploads a file to whats-th.is
        /// </summary>
        /// <param name="s">Stream containing the file to upload.</param>
        /// <param name="filename">Name of the file to upload.</param>
        /// <returns>Response from whats-th.is</returns>
        public async Task<Uri> UploadFileAsync(Stream s, string filename)
        {
            var dl = s.Length - s.Position;
            if (dl > 80 * 1024 * 1024 || dl <= 0)
                throw new ArgumentException("The data needs to be less than 80MiB and greather than 0B long.");

            var b64data = new byte[8];
            var rnd = new Random();
            for (var i = 0; i < b64data.Length; i++)
                b64data[i] = (byte)rnd.Next();

            var get_args = new Dictionary<string, string>
            {
                ["key"] = this.ApiKey
            };

            var turl = string.Concat(this.UploadEndpoint, "?", get_args.ToQueryString());

            var req = new HttpRequestMessage(HttpMethod.Post, new Uri(turl));
            var mpd = new MultipartFormDataContent(string.Concat("---upload-", Convert.ToBase64String(b64data), "---"));

            var bdata = new byte[dl];
            await s.ReadAsync(bdata, 0, bdata.Length);

            var fn = Path.GetFileName(filename);
            var sc = new ByteArrayContent(bdata);
            sc.Headers.ContentType = new MediaTypeHeaderValue(MimeTypeMap.GetMimeType(Path.GetExtension(fn)));

            mpd.Add(sc, "files[]", string.Concat("\"", fn.ToLower(), "\""));

            req.Content = mpd;

            var res = await this.HttpClient.SendAsync(req);

            var buff = await res.Content.ReadAsByteArrayAsync();
            var json = Encoding.GetString(buff, 0, buff.Length);

            var tfn = JsonConvert.DeserializeObject<OwoResponse>(json);

            var owof = tfn.Files.Count < 1 ? throw new Exception("OwO upload errored.") : tfn.Files.First();
            if (owof.Error == true)
                throw new Exception(string.Format("OwO upload failed with '{0}'.", owof.Description));

            return this.MakeUploadUri(owof.Url);
        }

#if !NETSTANDARD1_1
        /// <summary>
        /// Asynchronously uploads a file to whats-th.is
        /// </summary>
        /// <param name="fs">Stream containing the file to upload.</param>
        /// <returns>Response from whats-th.is</returns>
        public Task<Uri> UploadFileAsync(FileStream fs) =>
            this.UploadFileAsync(fs, Path.GetFileName(fs.Name));
#endif

        /// <summary>
        /// Makes a whats-th.is uri from given filename.
        /// </summary>
        /// <param name="fn">OwO filename.</param>
        /// <returns>Created URI.</returns>
        protected Uri MakeUploadUri(string fn)
        {
            var ub = new UriBuilder(this.Configuration.UploadUrl)
            {
                Path = string.Concat("/", fn)
            };

            return ub.Uri;
        }
        
        /// <summary>
        /// Makes a whats-th.is uri from given shortened url.
        /// </summary>
        /// <param name="fn">OwO shortened url.</param>
        /// <returns>Created URI.</returns>
        protected Uri MakeShortenedUri(string uri)
        {
            var ub = new UriBuilder(uri)
            {
                Host = this.Configuration.ShortenerUrl.Host
            };

            return ub.Uri;
        }

        /// <summary>
        /// Disposes this uploader instance.
        /// </summary>
        public void Dispose()
        {
            if (this._disposed)
                return;

            this._disposed = true;
            this.HttpClient.Dispose();
        }
    }
}
