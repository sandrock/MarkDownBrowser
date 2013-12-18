
namespace Srk.BrowseMark.LocalHttpServer
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using uhttpsharp;

    [HttpRequestHandlerAttributes("md")]
    public class MdHandler : HttpRequestHandler
    {
        public override HttpResponse Handle(HttpContext context)
        {
            var query = System.Web.HttpUtility.ParseQueryString(context.Request.Uri.Query);
            var path = query["path"];
            if (path == null)
            {
                return new uhttpsharp.HttpResponse(context, HttpResponseCode.BadRequest, "path is not specified", "text/plain; charset=utf-8");
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                return new uhttpsharp.HttpResponse(context, HttpResponseCode.BadRequest, "path is empty", "text/plain; charset=utf-8");
            }

            Encoding encoding = null;
            if (!string.IsNullOrWhiteSpace(query["encoding"]))
            {
                try
                {
                    encoding = Encoding.GetEncoding(query["encoding"].Trim());
                }
                catch (ArgumentException)
                {
                }
            }

            if (File.Exists(path))
            {
                string folder = Path.GetDirectoryName(path);
                try
                {
                    using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        StreamReader reader;
                        if (encoding != null)
                        {
                            reader = new StreamReader(fileStream, encoding);
                        }
                        else
                        {
                            reader = new StreamReader(fileStream, true);
                        }

                        string content = reader.ReadToEnd();
                        var mdOptions = new MarkdownSharp.MarkdownOptions
                        {
                            HyperlinkProcessor = s => GetDocumentsUrl(folder, s, context.Server.Port),
                            ImageLinkProcessor = s => PictureHandler.GetDocumentsUrl(Path.Combine(folder, s), context.Server.Port),
                        };
                        try
                        {
                            var md = new MarkdownSharp.Markdown(mdOptions);

                            var result = md.Transform(content);

                            var layout = this.GetLayout();
                            var variables = new Dictionary<string, Func<string>>();
                            variables.Add("/title///", () => System.Web.HttpUtility.HtmlEncode(path));
                            variables.Add("/content///", () => result);
                            variables.Add("/footer///", () => "");
                            var html = ProcessVariables(variables, layout, CultureInfo.InvariantCulture);
                            return new HttpResponse(context, HttpResponseCode.Found, html);
                        }
                        catch (Exception ex)
                        {
                            return new uhttpsharp.HttpResponse(context, HttpResponseCode.NotFound, "failed to process file" + Environment.NewLine + path + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine + "----" + Environment.NewLine + ex.ToString(), "text/plain; charset=utf-8");
                        }
                        
                        ////var memory = new MemoryStream();
                        ////fileStream.CopyTo(memory);
                        ////memory.Seek(0L, SeekOrigin.Begin);
                        ////return new HttpResponse(context, encoding.WebName, memory);
                    }
                }
                catch (Exception ex)
                {
                    return new uhttpsharp.HttpResponse(context, HttpResponseCode.NotFound, "failed to open file" + Environment.NewLine + path + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine + "----" + Environment.NewLine + ex.ToString(), "text/plain; charset=utf-8");
                }
            }
            else
            {
                return new uhttpsharp.HttpResponse(context, HttpResponseCode.NotFound, "file does not exist", "text/plain; charset=utf-8");
            }
        }

        private string GetLayout()
        {
            return this.GetAssemblyResourceAsText(this.GetType().Assembly, "Layout.html", Encoding.Unicode);
        }

        /// <summary>
        /// Processes the variables in a resource string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="culture">The culture.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">culture</exception>
        internal static string ProcessVariables(Dictionary<string, Func<string>> variables, string text, CultureInfo culture)
        {
            if (culture == null)
                throw new ArgumentNullException("culture");

            foreach (var var in variables)
            {
                text = text.Replace(var.Key, var.Value());
            }

            return text;
        }

        /// <summary>
        /// Gets the specified resource as text.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        private string GetAssemblyResourceAsText(Assembly assembly, string path, Encoding encoding)
        {
            path = assembly.GetName().Name + "." + path;
            var stream = assembly.GetManifestResourceStream(path);
            if (stream == null)
                return null;

            try
            {
                var reader = new StreamReader(stream, encoding);
                var result = reader.ReadToEnd();
                return result;
            }
            finally
            {
                stream.Close();
            }
        }

        public static string GetDocumentsUrl(string address, int port)
        {
            if (address.StartsWith("http://") || address.StartsWith("https://"))
                return address;

            address = Path.GetFullPath(address);
            address = address.Replace("/", "\\");
            return string.Format("http://127.0.0.1:{1}/md/?path={0}", Uri.EscapeDataString(address), port);
        }

        public static string GetDocumentsUrl(string folder, string filename, int port)
        {
            if (filename.StartsWith("http://") || filename.StartsWith("https://"))
                return filename;

            return GetDocumentsUrl(Path.Combine(folder, filename), port);
        }
    }
}
