// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using EnvDTE;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using Nerdbank.Streams;
using Newtonsoft.Json.Linq;
using StreamJsonRpc;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AngularLanguageService
{
    [ContentType("TypeScript")]
    [ContentType(AngularTemplateContentDefinition.Name)]
    [Export(typeof(ILanguageClient))]
    public class AngularLanguageClient : ILanguageClient, ILanguageClientCustomMessage2
    {
        public string Name => "Angular Language Service Extension";

        public IEnumerable<string> ConfigurationSections => null;

        public object InitializationOptions => null;

        public IEnumerable<string> FilesToWatch => null;

        public event AsyncEventHandler<EventArgs> StartAsync;
        public event AsyncEventHandler<EventArgs> StopAsync;

        [Import]
        private AngularLanguageServiceOutputPane outputPane { get; set; }

        public object MiddleLayer { get; }

        public object CustomMessageTarget => null;

        public AngularLanguageClient()
        {
             MiddleLayer = new MiddleLayerProvider(this);
        }

        public async Task<Connection> ActivateAsync(CancellationToken token)
        {
            await outputPane.WriteAsync("Activating language service.");

            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "node.exe";
            info.Arguments = 
                "" // " --inspect=9242"
                + " \"" + Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "node_modules\\@angular\\language-server\\index.js") + "\"" 
                // + " --logFile c:\\temp\\angularlsscript.txt"
                + " --logVerbosity verbose"
                + " --tsProbeLocations " + "\"" + Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "node_modules") + "\""
                + " --ngProbeLocations " + "\"" + Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "node_modules") + "\""
                + " --stdio";
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            
            var process = new System.Diagnostics.Process();
            process.StartInfo = info;
            process.ErrorDataReceived += (obj, data) => { outputPane.WriteAsync($"Error from node process: {data.Data}").Forget(); };

            await outputPane.WriteAsync("Starting node process.");

            try
            {
                if (process.Start())
                {
                    var inputPipe = new Pipe();
                    var input = process.StandardInput;
                    ForwardInput(inputPipe, input).Forget();

                    return new Connection(process.StandardOutput.BaseStream, inputPipe.Writer.AsStream());
                }
            }
            catch (Exception e)
            {
                ;
            }

            return null;
        }

        private System.Threading.Tasks.Task ForwardInput(Pipe inputPipe, StreamWriter input)
        {
            return System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        var readContent = await inputPipe.Reader.ReadAsync();
                        if (readContent.Buffer.Length == 0)
                        {
                            await System.Threading.Tasks.Task.Delay(100);
                        }
                        else
                        {
                            var content = readContent.Buffer.ToArray();
                            outputPane.WriteAsync($"[Client -> Server] {Encoding.UTF8.GetString(content)}").Forget();
                            inputPipe.Reader.AdvanceTo(readContent.Buffer.End);
                            await input.WriteAsync(Encoding.UTF8.GetString(content).ToCharArray());
                        }
                    }
                }
                catch (Exception e)
                {
                    throw;
                }
            });
        }

        public async System.Threading.Tasks.Task OnLoadedAsync()
        {
            await StartAsync.InvokeAsync(this, EventArgs.Empty).ConfigureAwait(false);
        }

        public System.Threading.Tasks.Task OnServerInitializeFailedAsync(Exception e)
        {
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public System.Threading.Tasks.Task OnServerInitializedAsync()
        {
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public System.Threading.Tasks.Task AttachForCustomMessageAsync(JsonRpc rpc)
        {
            return System.Threading.Tasks.Task.CompletedTask;
        }

        private class MiddleLayerProvider : ILanguageClientMiddleLayer
        {
            private readonly AngularLanguageClient parent;

            public MiddleLayerProvider(AngularLanguageClient parent)
            {
                this.parent = parent;
            }

            public bool CanHandle(string methodName)
            {
                switch (methodName)
                {
                    case "textDocument/completion":
                        return false; // Currently there's an issue with middle layer serialization of requests in the VS LSP framework
                    default:
                        return true;
                }
            }

            public System.Threading.Tasks.Task HandleNotificationAsync(string methodName, JToken methodParam, Func<JToken, System.Threading.Tasks.Task> sendNotification)
            {
                return sendNotification(methodParam);
            }

            public async Task<JToken> HandleRequestAsync(string methodName, JToken methodParam, Func<JToken, Task<JToken>> sendRequest)
            {
                await parent.outputPane.WriteAsync($"[Client -> Server][Middle Layer] {methodParam.ToString()}");
                var result = await sendRequest(methodParam);
                await parent.outputPane.WriteAsync($"[Client <- Server][Middle Layer] {result.ToString()}");
                return result;
            }
        }
    }
}