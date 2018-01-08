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
using System.Linq;
using System.Net;

namespace Emzi0767.OwoDotNet
{
    public static class Extensions
    {
        /// <summary>
        /// Converts a dictionary into a url-encoded query string.
        /// </summary>
        /// <param name="dict">Dictionary with query parameters.</param>
        /// <returns>Url-encoded query string.</returns>
        public static string ToQueryString(this IDictionary<string, string> dict)
        {
            if (dict == null || dict.Count == 0)
                return string.Empty;

            var vals_collection = dict.Select(xkvp => string.Concat(WebUtility.UrlEncode(xkvp.Key), "=", WebUtility.UrlEncode(xkvp.Value)));
            var vals = string.Join("&", vals_collection);

            return vals;
        }
    }
}
