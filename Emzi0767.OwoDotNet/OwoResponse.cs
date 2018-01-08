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

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Emzi0767.OwoDotNet
{
    /// <summary>
    /// Represents a response from whats-th.is.
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
    /// Represents a file uploaded to whats-th.is.
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
