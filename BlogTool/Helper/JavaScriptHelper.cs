using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BlogTool.Common;
using Newtonsoft.Json;

namespace BlogTool.Helper
{
    public class JavaScriptHelper
    {
        private static readonly string basePath = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;

        public static Task<string> ExportThumbnailAsync(ExportOption exportOption)
        {

            var tcs = new TaskCompletionSource<string>();
            var result = string.Empty;
            var phantomJS = new PhantomJSOverride();

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

        public static Task<string> GetHtmlFromMarkdownAsync(GetHtmlFromMarkdownOption option)
        {

            var tcs = new TaskCompletionSource<string>();
            var result = string.Empty;
            var phantomJS = new PhantomJSOverride();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                phantomJS.ToolPath = Path.Combine(basePath, "libs\\phantomjs-2.1.1-windows\\bin");

            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                phantomJS.ToolPath = Path.Combine(basePath, "libs\\phantomjs-2.1.1-linux-x86_64\\bin");

            }
            var scriptPath = Path.Combine(basePath, "js\\markdown.js");

            bool isCollectingOutput = false;
            var sb = new StringBuilder();

            phantomJS.OutputReceived += (sender, e) =>
            {
                Console.WriteLine("PhantomJS output: {0}", e.Data);
                if (!string.IsNullOrEmpty(e.Data) && e.Data.StartsWith("^^"))
                {

                    isCollectingOutput=true;
                }

                else if (!string.IsNullOrEmpty(e.Data) && e.Data.EndsWith("$$"))
                {
                    isCollectingOutput=false;
                    result = sb.ToString();
                    tcs.TrySetResult(result);
                }
                else if (!string.IsNullOrEmpty(e.Data) && isCollectingOutput)
                {
                    sb.AppendLine($"{e.Data}");
                }
            };
            phantomJS.ErrorReceived += (sender, e) =>
            {
                Console.WriteLine("PhantomJS error: {0}", e.Data);
                phantomJS.Abort();

            };
            var jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };


            var args = new string[] { };
            if (!string.IsNullOrEmpty(option.Markdown)) args = args.Concat(new string[] { "-markdown", option.Markdown }).ToArray();

            phantomJS.Run(scriptPath, args);
            return tcs.Task;
        }

    }
}
