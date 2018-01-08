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
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Emzi0767.OwoDotNet.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestUploadFile()
        {
            try
            {
                var cfg = new OwoConfiguration
                {
                    ApiKey = Environment.GetEnvironmentVariable("API_KEY")
                };

                using (var owo = new OwoClient(cfg))
                using (var fs = File.OpenRead("testimg.png"))
                {
                    var res = await owo.UploadFileAsync(fs);
                    
                    Assert.IsNotNull(res);
                }

            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public async Task TestUploadStream()
        {
            try
            {
                var cfg = new OwoConfiguration
                {
                    ApiKey = Environment.GetEnvironmentVariable("API_KEY")
                };

                using (var owo = new OwoClient(cfg))
                using (var fs = File.OpenRead("testimg.png"))
                using (var ms = new MemoryStream((int)fs.Length))
                {
                    await fs.CopyToAsync(ms);
                    ms.Position = 0;
                    
                    var res = await owo.UploadFileAsync(ms, "testimg.png");
                    
                    Assert.IsNotNull(res);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public async Task TestShortener()
        {
            try
            {
                var cfg = new OwoConfiguration
                {
                    ApiKey = Environment.GetEnvironmentVariable("API_KEY")
                };

                using (var owo = new OwoClient(cfg))
                {
                    var url = await owo.ShortenUrlAsync(new Uri("https://cdn.discordapp.com/attachments/244905538448523279/329285979225718794/1yd2emqfa16z.png"));

                    Assert.IsNotNull(url);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}
