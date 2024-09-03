﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BlogTool.Common;
using Newtonsoft.Json;
using NReco.PhantomJS;

namespace BlogTool.Helper
{
    public class ThumbnailHelper
    {
        private static readonly string basePath = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;

        public static Task<string> ExportAsync(ExportOption exportOption)
        {

            var tcs = new TaskCompletionSource<string>();
            var result = string.Empty;
            var phantomJS = new PhantomJS();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                phantomJS.ToolPath = Path.Combine(basePath, "libs\\phantomjs-2.1.1-windows\\bin");

            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                phantomJS.ToolPath = Path.Combine(basePath, "libs\\phantomjs-2.1.1-linux-x86_64\\bin");

            }
            var scriptPath = Path.Combine(basePath, "js\\thumbnail-converts.js");



            phantomJS.OutputReceived += (sender, e) =>
            {
                Console.WriteLine("PhantomJS output: {0}", e.Data);
                if (!string.IsNullOrEmpty(e.Data) && e.Data.StartsWith("render complete:"))
                {
                    var outfilePath = e.Data.Replace("render complete:", "");
                    Console.WriteLine(outfilePath);
                    result = outfilePath;
                    tcs.TrySetResult(result);
                }
            };
            phantomJS.ErrorReceived += (sender, e) =>
            {
                Console.WriteLine("PhantomJS error: {0}", e.Data);
                phantomJS.Abort();

            };
            var jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };


            var args = new string[] { };
            if (exportOption.Height > 0) args = args.Concat(new string[] { "-height", exportOption.Height.ToString() }).ToArray();
            if (exportOption.Width > 0) args = args.Concat(new string[] { "-width", exportOption.Width.ToString() }).ToArray();

            phantomJS.Run(scriptPath, args);
            return tcs.Task;
        }
    }
}
