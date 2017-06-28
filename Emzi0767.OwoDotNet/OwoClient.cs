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
        private static Encoding Encoding { get; set; } = new UTF8Encoding(false);

        /// <summary>
        /// Gets or sets the <see cref="HttpClient"/> instance to use for this uploader.
        /// </summary>
        protected HttpClient HttpClient { get; set; }

        /// <summary>
        /// Gets or sets the API key to use for uploading.
        /// </summary>
        protected string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the base API uri.
        /// </summary>
        protected Uri ApiBaseUri { get; set; }

        /// <summary>
        /// Gets or sets the upload endpoint.
        /// </summary>
        protected Uri UploadEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the shorten endpoint.
        /// </summary>
        protected Uri ShortenEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the client's configuration.
        /// </summary>
        protected OwoConfiguration Configuration { get; set; }

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

            this.Configuration = config;
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

            this.HttpClient = new HttpClient
            {
                BaseAddress = this.ApiBaseUri
            };

            this.HttpClient.DefaultRequestHeaders.Add("User-Agent", string.Concat("WhatsThisClient (https://github.com/Emzi0767/OwoDotNet, v", v.ToString(3), ")"));
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

            var turl = string.Concat(this.ShortenEndpoint, "?", this.MakeQueryString(get_args));

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
            if (dl > 80 * 1024 * 1024 || dl < 0)
                throw new ArgumentException("The data needs to be less than 80MiB and greather than 0B long.");

            var b64data = new byte[8];
            var rnd = new Random();
            for (var i = 0; i < b64data.Length; i++)
                b64data[i] = (byte)rnd.Next();

            var get_args = new Dictionary<string, string>
            {
                ["key"] = this.ApiKey
            };

            var turl = string.Concat(this.UploadEndpoint, "?", this.MakeQueryString(get_args));

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
            this.ApiKey = null;
        }

        private string MakeQueryString(IDictionary<string, string> args)
        {
            if (args == null || args.Count == 0)
                return string.Empty;

            var vals_collection = args.Select(xkvp => string.Concat(WebUtility.UrlEncode(xkvp.Key), "=", WebUtility.UrlEncode(xkvp.Value)));
            var vals = string.Join("&", vals_collection);

            return vals;
        }
    }
}
