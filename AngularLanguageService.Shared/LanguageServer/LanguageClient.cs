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
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using StreamJsonRpc;
#if VS2019
using Microsoft.VisualStudio.LanguageServer.Protocol;
#endif

namespace AngularLanguageService.LanguageServer
{
    /// <summary>
    /// <see cref="ILanguageClient"/> of the VS Angular Language Service extension.
    /// </summary>
    [Export(typeof(ILanguageClient))]
    [Export(AngularLanguageClientName, typeof(ILanguageClient))]
    [ContentType(AngularConstants.TypeScriptContentTypeName)]
    [ContentType(AngularConstants.AngularComponentContentTypeName)]
    internal sealed class LanguageClient : ILanguageClient, ILanguageClientCustomMessage2
    {
        internal const string AngularLanguageClientName = "Angular Language Service Extension";

        private static readonly string[] ConfigurationFiles = new string[] { "**/tsconfig.json" };

        private readonly AggregatingMiddleLayer aggregatingMiddleLayer;
#if VS2019
        private JsonRpc customMessageRpc;
#endif

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

#if VS2022
        bool ILanguageClient.ShowNotificationOnInitializeFailed => true;
#endif

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

#if VS2019
        Task ILanguageClient.OnServerInitializeFailedAsync(Exception exception) => Task.CompletedTask;
#elif VS2022
        Task<InitializationFailureContext> ILanguageClient.OnServerInitializeFailedAsync(ILanguageClientInitializationInfo initializationState)
        {
            var failureContext = new InitializationFailureContext { FailureMessage = initializationState.InitializationException.Message };
            return Task.FromResult(failureContext);
        }
#endif
#endregion

        #region ILanguageClientCustomMessage2 implementation
        object ILanguageClientCustomMessage2.MiddleLayer => this.aggregatingMiddleLayer;

        object ILanguageClientCustomMessage2.CustomMessageTarget => null;

        Task ILanguageClientCustomMessage2.AttachForCustomMessageAsync(JsonRpc rpc)
        {
#if VS2019
            customMessageRpc = rpc;
#endif
            return Task.CompletedTask;
        }
        #endregion

#if VS2019
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
#endif
    }
}
