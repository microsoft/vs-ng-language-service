// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using Newtonsoft.Json.Linq;
using StreamJsonRpc;
using Task = System.Threading.Tasks.Task;

namespace AngularLanguageService
{
    [ContentType("TypeScript")]
    [ContentType(AngularTemplateContentDefinition.Name)]
    [Export(typeof(ILanguageClient))]
    public class AngularLanguageClient : ILanguageClient, ILanguageClientCustomMessage2
    {
        private readonly AngularLanguageServiceOutputPane outputPane;

        public string Name => "Angular Language Service Extension";

        public IEnumerable<string> ConfigurationSections => null;

        public object InitializationOptions => null;

        public IEnumerable<string> FilesToWatch => null;

        public event AsyncEventHandler<EventArgs> StartAsync;
        public event AsyncEventHandler<EventArgs> StopAsync;

        public object MiddleLayer { get; }

        public object CustomMessageTarget => null;

        [ImportingConstructor]
        public AngularLanguageClient(AngularLanguageServiceOutputPane outputPane)
        {
            MiddleLayer = new MiddleLayerProvider(this);
            this.outputPane = outputPane;
        }

        public async Task<Connection> ActivateAsync(CancellationToken token)
        {
            outputPane.WriteAsync("Activating language service.").Forget();

            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "node.exe";
            info.Arguments =
                "" //+ " --inspect-brk=9242"
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

            var process = new Process();
            process.StartInfo = info;
            process.ErrorDataReceived += (obj, data) => { outputPane.WriteAsync($"Error from node process: {data.Data}").Forget(); };

            outputPane.WriteAsync("Starting node process.").Forget();

            try
            {
                if (process.Start())
                {
                    var inputPipe = new Pipe(new PipeOptions(useSynchronizationContext: false));
                    var input = process.StandardInput;
                    ForwardInputAsync(inputPipe, input).Forget();

                    return new Connection(process.StandardOutput.BaseStream, inputPipe.Writer.AsStream());
                }
            }
            catch (Exception)
            {
                // swallow exception
            }

            return null;
        }

        private async Task ForwardInputAsync(Pipe inputPipe, StreamWriter input)
        {
            await Task.Yield();

            while (true)
            {
                var readContent = await inputPipe.Reader.ReadAsync().ConfigureAwait(false);
                if (readContent.Buffer.Length == 0)
                {
                    await Task.Delay(100).ConfigureAwait(false);
                }
                else
                {
                    var content = BuffersExtensions.ToArray(readContent.Buffer);
                    outputPane.WriteAsync($"[Client -> Server] {Encoding.UTF8.GetString(content)}").Forget();
                    inputPipe.Reader.AdvanceTo(readContent.Buffer.End);
                    await input.WriteAsync(Encoding.UTF8.GetString(content).ToCharArray()).ConfigureAwait(false);
                }
            }
        }

        public async Task OnLoadedAsync()
        {
            await StartAsync.InvokeAsync(this, EventArgs.Empty).ConfigureAwait(false);
        }

        public Task OnServerInitializeFailedAsync(Exception e)
        {
            return Task.CompletedTask;
        }

        public Task OnServerInitializedAsync()
        {
            return Task.CompletedTask;
        }

        public Task AttachForCustomMessageAsync(JsonRpc rpc)
        {
            return Task.CompletedTask;
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

            public Task HandleNotificationAsync(string methodName, JToken methodParam, Func<JToken, Task> sendNotification)
            {
                return sendNotification(methodParam);
            }

            public async Task<JToken> HandleRequestAsync(string methodName, JToken methodParam, Func<JToken, Task<JToken>> sendRequest)
            {
                parent.outputPane.WriteAsync($"[Client -> Server][Middle Layer] {methodParam ?? "null"}").Forget();
                var result = await sendRequest(methodParam).ConfigureAwait(false);
                parent.outputPane.WriteAsync($"[Client <- Server][Middle Layer] {result ?? "null"}").Forget();
                return result;
            }
        }
    }
}