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

    [HttpRequestHandlerAttributes("css")]
    public class CssHandler : HttpRequestHandler
    {
        public override HttpResponse Handle(HttpContext context)
        {
            var query = System.Web.HttpUtility.ParseQueryString(context.Request.Uri.Query);

            // find file path from query string
            var name = query["name"];
            if (name == null)
            {
                return new uhttpsharp.HttpResponse(context, HttpResponseCode.NotFound, "name is not specified", "text/plain; charset=utf-8");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                return new uhttpsharp.HttpResponse(context, HttpResponseCode.NotFound, "name is empty", "text/plain; charset=utf-8");
            }

            var resource = GetAssemblyResourceAsText(this.GetType().Assembly, name + ".css", Encoding.Unicode);

            if (resource != null)
            {
                return new HttpResponse(context, HttpResponseCode.Ok, resource, contentType: "text/css; charset=utf-8");
            }
            else
            {
                // oops, no such file
                return new uhttpsharp.HttpResponse(context, HttpResponseCode.NotFound, "file does not exist", "text/plain; charset=utf-8");
            }
        }
    }
}
