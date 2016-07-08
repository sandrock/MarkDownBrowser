
namespace Srk.BrowseMark.WebApi
{
    using Srk.BrowseMark.Models;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using JsonConvert = Srk.BrowseMark.Common.JsonConvertEx;

    public class ApiClient : IDisposable
    {
        private static ApiClient local;
        private readonly string host;
        private readonly ushort minimumTcpPort;
        private readonly HttpClient discoverClient = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(5D),
        };
        private readonly HttpClient client = new HttpClient()
        {
#if DEBUG
            Timeout = TimeSpan.FromMinutes(10D),
#else
            Timeout = TimeSpan.FromSeconds(1D),
#endif
        };
        ////private ushort? actualPort;
        ////private ushort tentativePort;
        private bool isDisposed;

        public ApiClient()
            : this("127.0.0.1", AppConfiguration.Default.MinimumTcpPort)
        {
        }

        public ApiClient(string host)
            : this(host, AppConfiguration.Default.MinimumTcpPort)
        {
        }

        public ApiClient(string host, ushort minimumTcpPort)
        {
            this.host = host;
            this.minimumTcpPort = minimumTcpPort;
            ////this.tentativePort = minimumTcpPort;
        }

        public static ApiClient Local
        {
            get { return local ?? (local = new ApiClient()); }
        }

        public async Task<ushort?> DiscoverListeningPort()
        {
            for (ushort port = this.minimumTcpPort; port < ushort.MaxValue && port < (this.minimumTcpPort + 100); port++)
            {
                try
                {
                    var request = this.CreateRequest(port, HttpMethod.Get, "debug", null, null);
                    var authResponse = await this.discoverClient.SendAsync(request);
                    var jsonOut = await this.ReadResponse(authResponse);
                    ////return jsonOut;
                    return port;
                }
                catch (Exception ex)
                {
                    throw new ApiClientException(ex.Message, ex);
                }
            }

            return null;
        }

        public async Task<ushort[]> DiscoverListeningPorts()
        {
            var ports = new ConcurrentBag<ushort>();
            Parallel.For(this.minimumTcpPort, this.minimumTcpPort + 10, port =>
            {
                try
                {
                    var request = this.CreateRequest(checked((ushort)port), HttpMethod.Get, "debug", null, null);
                    var response = this.discoverClient.SendAsync(request).Result;
                    var jsonOut = this.ReadResponse(response).Result;
                    ports.Add(checked((ushort)port));
                }
                catch (Exception ex)
                {
                }
            });
            ////for (ushort port = this.minimumTcpPort; port < ushort.MaxValue && port < (this.minimumTcpPort + 100); port++)
            ////{
            ////}

            return ports.ToArray();
        }

        public async Task<DebugInfo> GetDebugInfo(ushort port)
        {
            try
            {
                var request = this.CreateRequest(port, HttpMethod.Get, "debug");
                var response = await this.client.SendAsync(request);
                var jsonOut = await this.ReadResponse(response);
                var obj = JsonConvert.DeserializeObject<DebugInfo>(jsonOut);
                return obj;
            }
            catch (Exception ex)
            {
                throw new ApiClientException(ex.Message, ex);
            }
        }

        public async Task<ProcessCommandResult> Command(ushort port, ProcessCommandRequest command)
        {
            try
            {
                var path = "command/?Id=" + command.Id + "&Command=" + Uri.EscapeDataString(command.Command) + "&Value=" + Uri.EscapeDataString(command.Value ?? "") + "&ProcessId=" + command.ProcessId;
                ////var jsonIn = JsonConvert.Serialize(command);
                var request = this.CreateRequest(port, HttpMethod.Get, path);
                var response = await this.client.SendAsync(request);
                var jsonOut = await this.ReadResponse(response);
                var obj = JsonConvert.DeserializeObject<ProcessCommandResult>(jsonOut);
                return obj;
            }
            catch (Exception ex)
            {
                throw new ApiClientException(ex.Message, ex);
            }
        }


        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    this.client.Dispose();
                }

                this.isDisposed = true;
            }
        }

        private HttpRequestMessage CreateRequest(ushort port, HttpMethod method, string path)
        {
            this.CheckDisposed();
            return this.CreateRequest(port, method, path, null, null);
        }

        private HttpRequestMessage CreateRequest(ushort port, HttpMethod method, string path, string jsonContent)
        {
            return this.CreateRequest(port, method, path, jsonContent, null);
        }

        private HttpRequestMessage CreateRequest(ushort port, HttpMethod method, string path, string content, string contentType)
        {
            var url = "http://" + this.host + ":" + port + "/" + path;
            var request = new HttpRequestMessage(method, url);
            request.Headers.Add("X-BrowseMark-ClientProcessId", Process.GetCurrentProcess().Id.ToString());

            if (content != null)
            {
                var baContent = new ByteArrayContent(Encoding.UTF8.GetBytes(content));
                baContent.Headers.Add("Content-Type", contentType ?? "application/json; charset=utf-8");
                request.Content = baContent;
            }

            return request;
        }

        private async Task<string> ReadResponse(HttpResponseMessage response)
        {
            // handle server errors
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ErrorResponse error = null;
                if (errorContent != null && errorContent.Length > 2)
                {
                    // there is a high probability we have a json error content here
                    try
                    {
                        error = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
                    }
                    catch (Exception jsonException)
                    {
                        // TODO: catch the right exception 
                        Trace.WriteLine("ApiClient.ReadResponse: jsonException = " + jsonException.ToString());
                    }
                }

                string message = "Request failed with error code " + ((int)response.StatusCode) + " " + response.StatusCode + ". ";
                if (response.StatusCode == HttpStatusCode.BadRequest && error != null)
                {
                    message = "Service error: "; ////+ (error.Message ?? error.Code);
                }
                else if (error != null)
                {
                    message = "Service error: "; ////+ (error.Message ?? error.Code);
                }

                var ex = new ApiClientException(message);
                ex.Data.Add("HttpCode", response.StatusCode);
                ex.Data.Add("Content", errorContent);
                ex.Data.Add("Error", error);

                throw ex;
            }

            var contentBytes = await response.Content.ReadAsByteArrayAsync();
            var contentString = Encoding.UTF8.GetString(contentBytes, 0, contentBytes.Length);

            return contentString;
        }

        private void CheckDisposed()
        {
            if (this.isDisposed)
                throw new ObjectDisposedException(this.ToString());
        }
    }
}
