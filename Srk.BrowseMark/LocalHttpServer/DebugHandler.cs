
namespace Srk.BrowseMark.LocalHttpServer
{
    using Srk.BrowseMark.Common;
    using Srk.BrowseMark.Models;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using uhttpsharp;

    [HttpRequestHandlerAttributes("debug")]
    public class DebugHandler : HttpRequestHandler
    {
        public override HttpResponse Handle(HttpContext context)
        {
            var obj = new DebugInfo();
            var process = Process.GetCurrentProcess();
            obj.ProcessId = process.Id;
            obj.ProcessSessionId = process.SessionId;

            var json = JsonConvertEx.Serialize(obj);

            return new HttpResponse(context, HttpResponseCode.Ok, json, "application/json; charset=utf-8");
        }
    }
}
