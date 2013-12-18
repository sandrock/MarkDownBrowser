
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
            var path = query["path"];
            if (path == null)
            {
                return new uhttpsharp.HttpResponse(context, HttpResponseCode.BadRequest, "path is not specified", "text/plain; charset=utf-8");
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                return new uhttpsharp.HttpResponse(context, HttpResponseCode.BadRequest, "path is empty", "text/plain; charset=utf-8");
            }

            if (File.Exists(path))
            {
                using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var memory = new MemoryStream();
                    fileStream.CopyTo(memory);
                    memory.Seek(0L, SeekOrigin.Begin);
                    return new HttpResponse(context, "image", memory);
                }
            }
            else
            {
                return new uhttpsharp.HttpResponse(context, HttpResponseCode.NotFound, "file does not exist", "text/plain; charset=utf-8");
            }
        }

        public static string GetDocumentsUrl(string address, int port)
        {
            address = Path.GetFullPath(address);
            address = address.Replace("/", "\\");
            return string.Format("http://127.0.0.1:{1}/pic/?path={0}", Uri.EscapeDataString(address), port);
        }
    }
}
