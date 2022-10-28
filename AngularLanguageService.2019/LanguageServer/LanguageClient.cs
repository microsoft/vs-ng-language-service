using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AngularLanguageService.Shared.LanguageServer;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using StreamJsonRpc;

namespace AngularLanguageService.LanguageServer
{
    /// <summary>
    /// <see cref="ILanguageClient"/> of the VS 2019 Angular Language Service extension.
    /// </summary>
    /// <remarks>
    /// This client exposes <see cref="GetAngularCompletionsAsync(CompletionParams)"/> for obtaining
    /// completions from the Angular server and returning them in <see cref="CompletionProvider"/> instead
    /// of using LSP.
    /// </remarks>
    [Export(typeof(ILanguageClient))]
    [Export(AngularLanguageClientName, typeof(ILanguageClient))]
    [ContentType(AngularConstants.TypeScriptContentTypeName)]
    [ContentType(AngularConstants.AngularComponentContentTypeName)]
    internal sealed class LanguageClient : ILanguageClient, ILanguageClientCustomMessage2
    {
        internal const string AngularLanguageClientName = "Angular Language Service Extension";

        private static readonly string[] ConfigurationFiles = new string[] { "**/tsconfig.json" };

        private readonly AggregatingMiddleLayer aggregatingMiddleLayer;
        private JsonRpc customMessageRpc;

        [ImportingConstructor]
        [Obsolete(AngularConstants.ImportingConstructorMessage, error: true)]
        internal LanguageClient(AggregatingMiddleLayer aggregatingMiddleLayer)
        {
            this.aggregatingMiddleLayer = aggregatingMiddleLayer;
        }

        #region ILanguageClient implementation
        public event AsyncEventHandler<EventArgs> /*ILanguageClient*/ StartAsync;

#pragma warning disable CS0067 // The event 'LanguageClient.StopAsync' is never used.
        public event AsyncEventHandler<EventArgs> /*ILanguageClient*/ StopAsync;
#pragma warning restore CS0067 // The event 'LanguageClient.StopAsync' is never used.

        string ILanguageClient.Name => AngularLanguageClientName;

        IEnumerable<string> ILanguageClient.ConfigurationSections => null;

        object ILanguageClient.InitializationOptions => null;

        IEnumerable<string> ILanguageClient.FilesToWatch => ConfigurationFiles;

        Task<Connection> ILanguageClient.ActivateAsync(CancellationToken token)
        {
            string dependenciesPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "node_modules");
            var startInfo = new ProcessStartInfo
            {
                // TODO: Should we try to find the right node path?
                FileName = "node.exe",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                Arguments =
                    $"\"{Path.Combine(dependenciesPath, "@angular", "language-server", "index.js")}\"" +
                    " --logVerbosity verbose" +
                    " --logToConsole" +
                    " --stdio" +
                    // TODO: Should we allow users to specify TypeScript location?
                    $" --tsProbeLocations \"{dependenciesPath}\"" +
                    $" --ngProbeLocations \"{dependenciesPath}\""
            };

            var process = new Process { StartInfo = startInfo };

            if (process.Start())
            {
                return Task.FromResult(new Connection(process.StandardOutput.BaseStream, process.StandardInput.BaseStream));
            }

            return Task.FromResult<Connection>(null);
        }

        Task ILanguageClient.OnLoadedAsync() => StartAsync.InvokeAsync(this, EventArgs.Empty);

        Task ILanguageClient.OnServerInitializedAsync() => Task.CompletedTask;

        Task ILanguageClient.OnServerInitializeFailedAsync(Exception exception) => Task.CompletedTask;
        #endregion

        #region ILanguageClientCustomMessage2 implementation
        object ILanguageClientCustomMessage2.MiddleLayer => aggregatingMiddleLayer;

        object ILanguageClientCustomMessage2.CustomMessageTarget => null;

        Task ILanguageClientCustomMessage2.AttachForCustomMessageAsync(JsonRpc rpc)
        {
            customMessageRpc = rpc;
            return Task.CompletedTask;
        }
        #endregion

        internal async Task<CompletionItem[]> GetAngularCompletionsAsync(CompletionParams completionParams)
        {
            if (customMessageRpc is not null && await customMessageRpc.InvokeWithParameterObjectAsync<CompletionItem[]>(Methods.TextDocumentCompletionName, completionParams) is CompletionItem[] completions)
            {
                return completions;
            }
            else
            {
                return Array.Empty<CompletionItem>();
            }
        }
    }
}
