using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Emzi0767.OwoDotNet
{
    /// <summary>
    /// Represents an uploader interface for whats-th.is
    /// </summary>
    public class OwoUploader : IDisposable
    {
        private const string POMF_UPLOAD = "/upload/pomf";
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

        private bool _disposed = false;

        /// <summary>
        /// Creates a new <see cref="OwoUploader"/>.
        /// </summary>
        /// <param name="api_key">API key to use for this uploader.</param>
        public OwoUploader(OwoConfiguration config, Version version)
        {
            this.ApiKey = config.ApiKey;
            this.ApiBaseUri = config.ApiBaseUri;

            var ub = new UriBuilder(this.ApiBaseUri)
            {
                Path = POMF_UPLOAD
            };

            this.HttpClient = new HttpClient
            {
                BaseAddress = ub.Uri
            };

            this.HttpClient.DefaultRequestHeaders.Add("User-Agent", string.Concat("WhatsThisClient (https://github.com/Emzi0767/OwoDotNet, v", version.ToString(3), ")"));
        }

        /// <summary>
        /// Asynchronously uploads a file to whats-th.is
        /// </summary>
        /// <param name="s">Stream containing the file to upload.</param>
        /// <param name="filename">Name of the file to upload.</param>
        /// <returns>Task representing the upload operation.</returns>
        public async Task<OwoResponse> UploadFileAsync(Stream s, string filename)
        {
            var dl = s.Length - s.Position;
            if (dl > 80 * 1024 * 1024 || dl < 0)
                throw new ArgumentException("The data needs to be less than 80MiB and greather than 0B long.");

            var turl = string.Concat(this.UploadEndpoint, "?key=", this.ApiKey);

            var req = new HttpRequestMessage(HttpMethod.Post, new Uri(turl));
            var mpd = new MultipartFormDataContent(string.Concat("upload------", DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss zzz")));

            var fn = Path.GetFileName(filename);
            var sc = new StreamContent(s);
            sc.Headers.ContentType = new MediaTypeHeaderValue(MimeTypeMap.GetMimeType(Path.GetExtension(fn)));

            mpd.Add(sc, "files[]", fn);

            req.Content = mpd;

            var res = await this.HttpClient.SendAsync(req);

            var buff = await res.Content.ReadAsByteArrayAsync();
            var json = Encoding.GetString(buff, 0, buff.Length);

            return JsonConvert.DeserializeObject<OwoResponse>(json);
        }

#if !NETSTANDARD1_1
        /// <summary>
        /// Asynchronously uploads a file to whats-th.is
        /// </summary>
        /// <param name="fs">Stream containing the file to upload.</param>
        /// <returns>Task representing the upload operation.</returns>
        public async Task<OwoResponse> UploadFileAsync(FileStream fs)
        {
            var dl = fs.Length - fs.Position;
            if (dl > 80 * 1024 * 1024 || dl < 0)
                throw new ArgumentException("The data needs to be less than 80MiB and greather than 0B long.");

            var turl = string.Concat(this.UploadEndpoint, "?key=", this.ApiKey);

            var req = new HttpRequestMessage(HttpMethod.Post, new Uri(turl));
            var mpd = new MultipartFormDataContent(string.Concat("upload------", DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss zzz")));

            var fn = Path.GetFileName(fs.Name);
            var sc = new StreamContent(fs);
            sc.Headers.ContentType = new MediaTypeHeaderValue(MimeTypeMap.GetMimeType(Path.GetExtension(fn)));

            mpd.Add(sc, "files[]", fn);

            req.Content = mpd;

            var res = await this.HttpClient.SendAsync(req);

            var buff = await res.Content.ReadAsByteArrayAsync();
            var json = Encoding.GetString(buff, 0, buff.Length);

            return JsonConvert.DeserializeObject<OwoResponse>(json);
        }
#endif

        /// <summary>
        /// Makes a whats-th.is uri from given filename and config.
        /// </summary>
        /// <param name="fn">OwO filename.</param>
        /// <param name="cfg">OwO configuration.</param>
        /// <returns>Created URI.</returns>
        public Uri MakeUri(string fn, OwoConfiguration cfg)
        {
            var ub = new UriBuilder(cfg.PreferredUri)
            {
                Path = string.Concat("/", fn)
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
    }
}
