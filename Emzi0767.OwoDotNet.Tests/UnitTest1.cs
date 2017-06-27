using System;
using System.IO;
using System.Linq;
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
                var dcfg = OwoConfiguration.CreateDefault();
                var cfg = new OwoConfiguration
                {
                    ApiBaseUri = dcfg.ApiBaseUri,
                    ShortenerUrl = dcfg.ShortenerUrl,
                    UploadUrl = dcfg.UploadUrl,
                    ApiKey = Environment.GetEnvironmentVariable("API_KEY")
                };

                using (var owo = new OwoClient(cfg))
                using (var fs = File.OpenRead("testimg.png"))
                {
                    var res = await owo.UploadFileAsync(fs);
                    
                    Assert.IsNotNull(res.Files);
                    Assert.AreNotEqual(0, res.Files.Count);

                    var owof = res.Files.First();

                    Assert.IsNotNull(owof.Url);

                    var url = owo.MakeUri(owof.Url);

                    Assert.IsNotNull(url);

                    Console.WriteLine(url);
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
                var dcfg = OwoConfiguration.CreateDefault();
                var cfg = new OwoConfiguration
                {
                    ApiBaseUri = dcfg.ApiBaseUri,
                    ShortenerUrl = dcfg.ShortenerUrl,
                    UploadUrl = dcfg.UploadUrl,
                    ApiKey = Environment.GetEnvironmentVariable("API_KEY")
                };

                using (var owo = new OwoClient(cfg))
                using (var fs = File.OpenRead("testimg.png"))
                using (var ms = new MemoryStream((int)fs.Length))
                {
                    await fs.CopyToAsync(ms);
                    ms.Position = 0;
                    
                    var res = await owo.UploadFileAsync(ms, "testimg.png");

                    Assert.IsNotNull(res.Files);
                    Assert.AreNotEqual(0, res.Files.Count);

                    var owof = res.Files.First();

                    Assert.IsNotNull(owof.Url);

                    var url = owo.MakeUri(owof.Url);

                    Assert.IsNotNull(url);

                    Console.WriteLine(url);
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
                var dcfg = OwoConfiguration.CreateDefault();
                var cfg = new OwoConfiguration
                {
                    ApiBaseUri = dcfg.ApiBaseUri,
                    ShortenerUrl = dcfg.ShortenerUrl,
                    UploadUrl = dcfg.UploadUrl,
                    ApiKey = Environment.GetEnvironmentVariable("API_KEY")
                };

                using (var owo = new OwoClient(cfg))
                {
                    var url = await owo.ShortenUrlAsync(new Uri("https://cdn.discordapp.com/attachments/244905538448523279/329285979225718794/1yd2emqfa16z.png"));

                    Assert.IsNotNull(url);

                    Console.WriteLine(url);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}
