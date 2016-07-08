
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

    [HttpRequestHandlerAttributes("command")]
    public class CommandHandler : HttpRequestHandler
    {
        public override HttpResponse Handle(HttpContext context)
        {
            ////if (context.Request.HttpMethod != HttpMethod.Post)
            ////{
            ////    var error = new ErrorResponse();
            ////    error.Code = "HttpMethodIsNotSupported";
            ////    return new HttpResponse(context, HttpResponseCode.NotFound, JsonConvertEx.Serialize(error), "application/json; charset=utf-8");
            ////}

            var query = System.Web.HttpUtility.ParseQueryString(context.Request.Uri.Query);
            var command = new ProcessCommandRequest();
            command.Command = query["Command"];
            Guid id;
            command.Id = Guid.TryParse(query["Id"], out id) ? id : default(Guid?);
            int processId;
            command.ProcessId = int.TryParse(query["ProcessId"], out processId) ? processId : default(int?);
            command.Value = query["Value"];

            var result = ProcessCommandResult.FromRequest(command, false);

            if ("BringToFront".Equals(command.Command, StringComparison.OrdinalIgnoreCase))
            {
                App.Current.Dispatcher.BeginInvoke(() =>
                {
                    var window = App.Current.MainWindow;
                    if (window.WindowState == System.Windows.WindowState.Minimized)
                        window.WindowState = System.Windows.WindowState.Normal;
                    window.Show();
                });
                result.Success = true;
            }
            else if ("OpenFile".Equals(command.Command, StringComparison.OrdinalIgnoreCase))
            {
                App.Current.Dispatcher.BeginInvoke(() =>
                {
                    var window = App.Current.MainWindow;
                    var main = (MainWindow)window;
                    main.Load(command.Value);
                });
                result.Success = true;
            }
            else
            {
                result.Code = "UnknownCommand";
            }

            return new HttpResponse(context, HttpResponseCode.Ok, JsonConvertEx.Serialize(result), "application/json; charset=utf-8");
        }
    }
}
