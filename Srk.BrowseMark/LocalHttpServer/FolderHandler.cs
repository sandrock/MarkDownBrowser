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
    using System.Text;
    using uhttpsharp;

    [HttpRequestHandlerAttributes("folder")]
    public class FolderHandler : HttpRequestHandler
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

            if (Directory.Exists(path))
            {
                var variables = new Dictionary<string, Func<string>>();
                variables.Add("/title///", () => System.Web.HttpUtility.HtmlEncode(path));
                variables.Add("/content///", () => "<p>Please select a file</p>");
                variables.Add("/footer///", () => "");

                try
                {
                    string navMd = GetFolderContentsAsMarkdown(path, context);
                    // pass it through markdown
                    var mdOptions = new MarkdownSharp.MarkdownOptions
                    {
                        HyperlinkProcessor = s => MdHandler.GetDocumentsUrl(path, s, context.Server.Port),
                        ImageLinkProcessor = s => PictureHandler.GetDocumentsUrl(Path.Combine(path, s), context.Server.Port),
                    };
                    var md = new MarkdownSharp.Markdown(mdOptions);
                    var result = md.Transform(navMd);
                    variables.Add("/nav///", () => result);

                    // open layout from assembly's ressources, replace a few variables
                    var layout = MdHandler.GetLayout();
                    var html = MdHandler.ProcessVariables(variables, layout, CultureInfo.InvariantCulture);

                    // and we're done
                    return new HttpResponse(context, HttpResponseCode.Ok, html);
                }
                catch (Exception ex)
                {
                    // oops, file error
                    return new uhttpsharp.HttpResponse(context, HttpResponseCode.NotFound, "failed to open directory" + Environment.NewLine + path + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine + "----" + Environment.NewLine + ex.ToString(), "text/plain; charset=utf-8");
                }
            }
            else
            {
                // oops, no such file
                return new uhttpsharp.HttpResponse(context, HttpResponseCode.NotFound, "directory does not exist", "text/plain; charset=utf-8");
            }
        }

        internal static string GetFolderContentsAsMarkdown(string path, HttpContext context)
        {
            var folders = Directory.GetDirectories(path);
            var files = Directory.GetFiles(path);

            var sb = new StringBuilder();
            sb.AppendLine("## Folders");
            sb.AppendLine();
            if (path.Length > 3)
            {
                sb.AppendLine(
                    string.Format("[{0}]({1})",
                    "../",
                    FolderHandler.GetFoldersUrl(Path.GetDirectoryName(path), context.Server.Port)));
            }

            if (folders.Length > 0)
            {
                for (int i = 0; i < folders.Length; i++)
                {
                    sb.AppendLine(
                        string.Format("[{0}/]({1})",
                        Path.GetFileName(folders[i]),
                        FolderHandler.GetFoldersUrl(folders[i], context.Server.Port)));
                }
            }

            if (files.Length > 0)
            {
                sb.AppendLine("## Files");
                sb.AppendLine();
                for (int i = 0; i < files.Length; i++)
                {
                    sb.AppendLine(
                        string.Format("[{0}]({1})",
                        Path.GetFileName(files[i]),
                        MdHandler.GetDocumentsUrl(files[i], context.Server.Port)));
                }
            }

            return sb.ToString();
        }

        public static string GetFoldersUrl(string address, int port)
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
            return string.Format("http://127.0.0.1:{1}/folder/?path={0}", Uri.EscapeDataString(address), port);
        }
    }
}
