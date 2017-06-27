using System.Collections.Generic;
using Newtonsoft.Json;

namespace Emzi0767.OwoDotNet
{
    /// <summary>
    /// Represents a response from whats-th.is
    /// </summary>
    public struct OwoResponse
    {
        /// <summary>
        /// Gets the returned files.
        /// </summary>
        [JsonProperty("files")]
        public IReadOnlyList<OwoFile> Files { get; internal set; }
    }

    /// <summary>
    /// Represents a file uploaded to whats-th.is
    /// </summary>
    public struct OwoFile
    { 
        /// <summary>
        /// Gets the returned file name.
        /// </summary>
        [JsonProperty("name")]
        public string FileName { get; internal set; }

        /// <summary>
        /// Gets the hash of the file.
        /// </summary>
        [JsonProperty("hash")]
        public string Hash { get; internal set; }

        /// <summary>
        /// Gets the file's URL.
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; internal set; }

        /// <summary>
        /// Gets the file's length.
        /// </summary>
        [JsonProperty("length")]
        public long Length { get; internal set; }

        /// <summary>
        /// Gets whether the upload errored.
        /// </summary>
        [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Error { get; internal set; }

        /// <summary>
        /// Gets the description of the error, if applicable.
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get;internal set; }
    }
}
