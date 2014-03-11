// MarkDownBrowser
// Copyright 2013 SandRock
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

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

            // find file path from query string
            var path = query["path"];
            if (path == null)
            {
                return new uhttpsharp.HttpResponse(context, HttpResponseCode.NotFound, "path is not specified", "text/plain; charset=utf-8");
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                return new uhttpsharp.HttpResponse(context, HttpResponseCode.NotFound, "path is empty", "text/plain; charset=utf-8");
            }

            // find custom encoding from query string
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

            // check file
            if (File.Exists(path))
            {
                string folder = Path.GetDirectoryName(path);

                var variables = new Dictionary<string, Func<string>>();
                variables.Add("/title///", () => System.Web.HttpUtility.HtmlEncode(path));
                variables.Add("/footer///", () => "");

                var navMd = FolderHandler.GetFolderContentsAsMarkdown(folder, context);
                {
                    // pass it through markdown
                    var mdOptions = new MarkdownSharp.MarkdownOptions
                    {
                        HyperlinkProcessor = s => MdHandler.GetDocumentsUrl(folder, s, context.Server.Port),
                        ImageLinkProcessor = s => PictureHandler.GetDocumentsUrl(Path.Combine(folder, s), context.Server.Port),
                    };
                    var md = new MarkdownSharp.Markdown(mdOptions);
                    var result = md.Transform(navMd);
                    variables.Add("/nav///", () => result);
                }

                try
                {
                    // open and read file
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

                        // pass it through markdown
                        var mdOptions = new MarkdownSharp.MarkdownOptions
                        {
                            HyperlinkProcessor = s => MdHandler.GetDocumentsUrl(folder, s, context.Server.Port),
                            ImageLinkProcessor = s => PictureHandler.GetDocumentsUrl(Path.Combine(folder, s), context.Server.Port),
                        };
                        try
                        {
                            var md = new MarkdownSharp.Markdown(mdOptions);
                            var result = md.Transform(content);
                            variables.Add("/content///", () => result);

                            // open layout from assembly's ressources, replace a few variables
                            var layout = GetLayout();
                            var html = ProcessVariables(variables, layout, CultureInfo.InvariantCulture);

                            // and we're done
                            return new HttpResponse(context, HttpResponseCode.Ok, html);
                        }
                        catch (Exception ex)
                        {
                            // oops, parse error
                            return new uhttpsharp.HttpResponse(context, HttpResponseCode.NotFound, "failed to process file" + Environment.NewLine + path + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine + "----" + Environment.NewLine + ex.ToString(), "text/plain; charset=utf-8");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // oops, file error
                    return new uhttpsharp.HttpResponse(context, HttpResponseCode.NotFound, "failed to open file" + Environment.NewLine + path + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine + "----" + Environment.NewLine + ex.ToString(), "text/plain; charset=utf-8");
                }


            }
            else
            {
                // oops, no such file
                return new uhttpsharp.HttpResponse(context, HttpResponseCode.NotFound, "file does not exist", "text/plain; charset=utf-8");
            }
        }

        internal static string GetLayout()
        {
            return GetAssemblyResourceAsText(typeof(MdHandler).Assembly, "Layout.html", Encoding.Unicode);
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

        public static string GetDocumentsUrl(string address, int port)
        {
            // avoid altering URLs
            if (address.StartsWith("http://") || address.StartsWith("https://"))
                return address;

            // now we have to make the navigation URL for a MD file
            // the url start with the webserver's address
            // uses the /md/ handler (this class)
            // and the path to the file is passed as a query string "path"
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
