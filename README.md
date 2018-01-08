# OwO.NET [![Build status](https://ci.appveyor.com/api/projects/status/ff040v0m608g6uui/branch/master?svg=true)](https://ci.appveyor.com/project/Emzi0767/owodotnet/branch/master)

A .NET Standard-compatible wrapper for [OwO](https://owo.whats-th.is/).

### Compatible .NET versions:

The library is compatible with following versions of .NET Framework:

- .NET Framework 4.5
- .NET Standard 1.1
- .NET Standard 2.0

See [here](https://blogs.msdn.microsoft.com/dotnet/2016/09/26/introducing-net-standard/) for details about .NET Standard.

### Installation

Install [`Emzi0767.OwoDotNet`](https://www.nuget.org/packages/Emzi0767.OwoDotNet) 
from NuGet.

### Examples

Below are examples of the library's usage.

Note: if you intend to perform several operations, it's a good idea to perform them using the same OwO client.

#### Loading client configuration

Easiest way to load client configuration is to parse it from an external source, like a json file. This example shows how 
to do that.

```cs
var cfgs = "{}";
using (var fs = File.OpenRead(my_config_path))
using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
    cfgs = sr.ReadToEnd();

var cfg = JsonConvert.DeserializeObject<OwoConfiguration>(cfgs);
```

#### Uploading a file

Once configuration is loaded, you can start uploading data. Here's an example demonstrating file uploads. It will upload
a file and print out its url to the console.

```cs
using (var owo = new OwoClient(cfg))
using (var fs = File.OpenRead("testimg.png"))
{
    var url = await owo.UploadFileAsync(fs);
    Console.WriteLine(url);
}
```

#### Uploading from a stream

Uploading data from a stream is very similar, except you need to specify a file name to upload.

```cs
using (var owo = new OwoClient(cfg))
using (var s = my_stream)
{
    var url = await owo.UploadFileAsync(s, "testimg.png");
    Console.WriteLine(url);
}
```

#### Shortening links

The library also offers an interface for the link shortener. Usage is just as simple.

```cs
using (var owo = new OwoClient(cfg))
{
    var url = await owo.ShortenUrlAsync(new Uri(my_url));
    Console.WriteLine(url);
}
```

### Contributing

Pull requests are accepted. Make sure you add test suites for new features and
make sure the code passes the spec (so the build doesn't break). Tests are
automatically run when commits are made in a pull request.

### License

The contents of this repository are licensed under Apache License 2.0. A copy of 
the Apache License 2.0 can be found in [the LICENSE.txt file](https://github.com/Emzi0767/OwoDotNet/blob/master/LICENSE.txt).