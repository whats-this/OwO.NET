using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Emzi0767.OwoDotNet
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var a = typeof(Program)
                .GetTypeInfo()
                .Assembly;
            var an = a.Location;
            var v = a.GetName().Version;

            var utf8 = new UTF8Encoding(false);
            var apth = Path.GetDirectoryName(an);
            var cpth = Path.Combine(apth, ".owo_config.json");

            Console.WriteLine("OwO.NET, version {0}", v.ToString(3));
            Console.WriteLine("by Emzi0767");
            Console.WriteLine("https://github.com/Emzi0767/OwoDotNet");
            Console.WriteLine("-------------------------------------");

            // ...
            if (!File.Exists(cpth))
            {
                // config does not exist, create a new one

                var dcfg = OwoConfiguration.CreateDefault();

                var dcfgs = JsonConvert.SerializeObject(dcfg);

                using (var fs = File.Create(cpth))
                using (var sw = new StreamWriter(fs, utf8))
                    sw.Write(dcfgs);

                Console.WriteLine("Config file was not found. A new one was created at '{0}'. Fill it with necessary data and rerun this uploader.", cpth);
                return;

                // full list of URLs is available at:
                // https://whats-th.is/faq.html#how-to-access-files
            }

            OwoConfiguration cfg = default(OwoConfiguration);
            try
            {
                var cfgs = "{}";
                using (var fs = File.OpenRead(cpth))
                using (var sr = new StreamReader(fs, utf8))
                    cfgs = sr.ReadToEnd();

                cfg = JsonConvert.DeserializeObject<OwoConfiguration>(cfgs);

                if (string.IsNullOrWhiteSpace(cfg.ApiKey) || cfg.ApiKey == "your-api-key-here" || cfg.UploadUrl == null)
                    throw new InvalidDataException();
            }
            catch (InvalidDataException)
            {
                Console.WriteLine("Invalid data detected in your config file located at '{0}'. Make sure all the settings are set correctly.", cpth);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine("There was a problem reading config file from '{0}'. Ensure the config is a valid JSON file, and all the required values are set.", cpth);
                Console.WriteLine("{0}: {1}", ex.GetType(), ex.Message);
                return;
            }

            RunUploaderAsync(cfg, v, apth, args).GetAwaiter().GetResult();
        }

        private static async Task RunUploaderAsync(OwoConfiguration cfg, Version v, string fbp, string[] args)
        {
            try
            {
                var ups = new Dictionary<string, Uri>();

                using (var owo = new OwoClient(cfg))
                {
                    foreach (var arg in args)
                    {
                        if (!File.Exists(arg))
                            throw new IOException(string.Concat("Specified file ('", arg, "') does not exist."));

                        var fi = new FileInfo(arg);
                        Console.WriteLine("Attempting to upload '{0}'...", Path.GetFileName(fi.Name));

                        var tfn = default(OwoResponse);
                        using (var fs = fi.OpenRead())
                            tfn = await owo.UploadFileAsync(fs);

                        var owof = tfn.Files.Count < 1 ? throw new Exception("OwO upload errored.") : tfn.Files.First();
                        if (owof.Error == true)
                            throw new Exception(string.Format("OwO upload failed with '{0}'.", owof.Description));

                        var turi = owo.MakeUri(owof.Url);
                        Console.WriteLine("'{0}' uploaded to '{1}'", fi.Name, turi);

                        ups[fi.FullName] = turi;
                    }
                }

                var rpth = AppContext.BaseDirectory;
                if (string.IsNullOrWhiteSpace(rpth) || !Directory.Exists(rpth))
                    rpth = fbp;
                rpth = Path.Combine(rpth, string.Concat("upload-", DateTimeOffset.Now.ToString("yyyy-MM-dd-HH-mm-ss"), ".txt"));

                Console.WriteLine("Writing upload report to '{0}'", rpth);

                using (var fs = File.Create(rpth))
                using (var sw = new StreamWriter(fs, new UTF8Encoding(false)))
                    foreach (var xkvp in ups)
                        sw.WriteLine("'{0}' = '{1}'", xkvp.Key, xkvp.Value);

            }
            catch (Exception ex)
            {
                Console.WriteLine("There was a problem uploading your file(s) to owo. Ensure that all the supplied paths are correct, and that your API key is valid.");
                Console.WriteLine("{0}: {1}", ex.GetType(), ex.Message);
                return;
            }
        }
    }
}