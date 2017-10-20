// The MIT License (MIT)

// Copyright (c) 2016 Ben Abelshausen

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sample.Shape
{
    /// <summary>
    /// Contains code to download test files.
    /// </summary>
    public static class Download
    {
        /// <summary>
        /// Downloads a file if it doesn't exist yet.
        /// </summary>
        public static async Task ToFile(string url, string filename)
        {
            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename)))
            {
                var client = new HttpClient();
                client.Timeout = new TimeSpan(0, 10, 0);
                using (var stream = await client.GetStreamAsync(url))
                using (var outputStream = File.OpenWrite(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename)))
                {
                    stream.CopyTo(outputStream);
                }
            }
        }
        
        /// <summary>
        /// Downloads and extracts the given file.
        /// </summary>
        public static void DownloadAndExtractShape(string url, string filename)
        {
            Download.ToFile(url, filename).Wait();
            Extract(filename);
        }

        /// <summary>
        /// Extracts the given file to a 'temp' directory.
        /// </summary>
        /// <param name="file"></param>
        public static void Extract(string file)
        {
            var archive = new ZipArchive(File.OpenRead(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file)));
            var baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");
            if (!Directory.Exists(baseDir))
            {
                Directory.CreateDirectory(baseDir);
            }
            foreach (var entry in archive.Entries)
            {
                if (!string.IsNullOrWhiteSpace(entry.Name))
                {
                    var entryFile = Path.Combine(baseDir, entry.FullName);
                    using (var entryStream = entry.Open())
                    using (var outputStream = File.OpenWrite(entryFile))
                    {
                        entryStream.CopyTo(outputStream);
                    }
                }
                else
                {
                    var dir = Path.Combine(baseDir, entry.FullName);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                }
            }
        }
    }
}
