
namespace Srk.BrowseMark
{
    using Srk.BrowseMark.Models;
    using Srk.BrowseMark.WebApi;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    static class AppEntryPoint
    {
        [System.STAThreadAttribute()]
        public static void Main(string[] args)
        {
            var files = new List<string>();
            if (args != null && args.Length > 0)
            {
                foreach (var arg in args)
                {
                    if (System.IO.File.Exists(arg))
                    {
                        files.Add(System.IO.Path.GetFullPath(arg));
                    }
                }
            }

            var myProcess = System.Diagnostics.Process.GetCurrentProcess();
            var allMyProcesses = new List<System.Diagnostics.Process>();
            allMyProcesses.AddRange(System.Diagnostics.Process.GetProcessesByName(myProcess.ProcessName));
            allMyProcesses.AddRange(System.Diagnostics.Process.GetProcessesByName(myProcess.ProcessName.Contains(".vshost") ? myProcess.ProcessName.Replace(".vshost", string.Empty) : (myProcess.ProcessName + ".vshost")));
            var myProcesses = new List<System.Diagnostics.Process>();
            var myProcessesPorts = new List<ushort>();

            foreach (var process in allMyProcesses)
            {
                if (process.Id.Equals(myProcess.Id) || !process.SessionId.Equals(myProcess.SessionId))
                    continue;

                ////process.StandardInput.WriteLine("NEW INSTANCE " + myProcess.Id);
                myProcesses.Add(process);
            }

            var client = ApiClient.Local;
            if (myProcesses.Count > 0)
            {
                myProcessesPorts.AddRange(client.DiscoverListeningPorts().Result);
            }

            if (files.Count > 0 && myProcessesPorts.Count > 0)
            {
                var process = myProcessesPorts[0];

                var processOutputs = new List<string>();
                var processOutputResults = new List<ProcessCommandResult>();
                var commands = files.Select(file => ProcessCommandRequest.OpenFile(file, myProcess.Id)).ToArray();
                try
                {
                    foreach (var command in commands)
                    {
                        ProcessCommandResult result = null;
                        Guid id = command.Id.Value;
                        try
                        {
                            var task = client.Command(process, command);
                            task.Wait();
                            result = task.Result;
                        }
                        catch (ApiClientException ex)
                        {
                            Trace.WriteLine("App.OnStartup/InvokePriorProcess/SendCommand: got no result for command " + id + " execution: " + ex.Message);
                            continue;
                        }

                        if (result != null)
                        {
                            if (result.Success)
                            {
                                Trace.WriteLine("App.OnStartup/InvokePriorProcess/SendCommand: command " + id + " was executed.");
                            }
                            else
                            {
                                Trace.WriteLine("App.OnStartup/InvokePriorProcess/SendCommand: command " + id + " was NOT executed.");
                            }
                        }
                        else
                        {
                            Trace.WriteLine("App.OnStartup/InvokePriorProcess/SendCommand: got no result for command " + id + " execution. ");
                            continue;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("App.OnStartup/InvokePriorProcess/SendCommand: ERROR " + ex.Message);
                }
                finally
                {
                }

                return;
            }
            else if (myProcessesPorts.Count > 0)
            {
                foreach (var process in myProcessesPorts)
                {
                    var processOutputs = new List<string>();
                    var processOutputResults = new List<ProcessCommandResult>();
                    var command = ProcessCommandRequest.BringToFront(myProcess.Id);
                    try
                    {
                        ProcessCommandResult result = null;
                        Guid id = command.Id.Value;
                        try
                        {
                            var task = client.Command(process, command);
                            task.Wait();
                            result = task.Result;
                        }
                        catch (ApiClientException ex)
                        {
                            Trace.WriteLine("App.OnStartup/InvokePriorProcess/SendCommand: got no result for command " + id + " execution: " + ex.Message);
                            continue;
                        }


                        if (result != null)
                        {
                            if (result.Success)
                            {
                                Trace.WriteLine("App.OnStartup/InvokePriorProcess/SendCommand: command " + id + " was executed.");
                                return;
                            }
                            else
                            {
                                Trace.WriteLine("App.OnStartup/InvokePriorProcess/SendCommand: command " + id + " was NOT executed.");
                            }
                        }
                        else
                        {
                            Trace.WriteLine("App.OnStartup/InvokePriorProcess/SendCommand: got no result for command " + id + " execution. ");
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine("App.OnStartup/InvokePriorProcess/SendCommand: ERROR " + ex.Message);
                    }
                    finally
                    {
                    }
                }
            }

            var app = new App();
            app.InitializeComponent();
            Trace.WriteLine("AppEntryPoint.Main: calling app.Run.");
            app.Run();
            Trace.WriteLine("AppEntryPoint.Main: app.Run ended.");
            Trace.WriteLine("AppEntryPoint.Main: the end.");
        }
    }
}
