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
    using System.IO;
    using System.Linq;
    using System.Text;
    using uhttpsharp;

    [HttpRequestHandlerAttributes("pic")]
    public class PictureHandler : HttpRequestHandler
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

            // check file
            if (File.Exists(path))
            {
                try
                {
                    using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        // it's important to use a memorystream because the http response will be served at a later time
                        // and the fileStream will already be disposed
                        // the gc will do a good work at destroying it
                        var memory = new MemoryStream();
                        fileStream.CopyTo(memory);
                        memory.Seek(0L, SeekOrigin.Begin);
                        return new HttpResponse(context, "image", memory);
                        // TODO: the Content-Type served "image" is not the greatest of all
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

        public static string GetDocumentsUrl(string address, int port)
        {
            // avoid altering URLs
            if (address.StartsWith("http://") || address.StartsWith("https://"))
                return address;

            address = Path.GetFullPath(address);
            address = address.Replace("/", "\\");
            return string.Format("http://127.0.0.1:{1}/pic/?path={0}", Uri.EscapeDataString(address), port);
        }
    }
}
