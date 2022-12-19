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
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
#if VS2019
using Microsoft.VisualStudio.LanguageServer.Protocol;
#endif
using Task = System.Threading.Tasks.Task;

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

        private readonly SVsServiceProvider serviceProvider;
        private readonly AggregatingMiddleLayer aggregatingMiddleLayer;
#if VS2019
        private JsonRpc customMessageRpc;
#endif

        [ImportingConstructor]
        [Obsolete(AngularConstants.ImportingConstructorMessage, error: true)]
        internal LanguageClient(SVsServiceProvider serviceProvider, AggregatingMiddleLayer aggregatingMiddleLayer)
        {
            this.serviceProvider = serviceProvider;
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

        async Task<Connection> ILanguageClient.ActivateAsync(CancellationToken token)
        {
            // Use both the current solution and the extension's bundle as probe locations.
            string solutionPath = await ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                var solution = this.serviceProvider.GetService<SVsSolution, IVsSolution>();
                if (solution.GetSolutionInfo(out var solutionPath, out _, out _) == VSConstants.S_OK)
                {
                    return solutionPath.TrimSuffix(Path.DirectorySeparatorChar.ToString()) + ",";
                }

                return string.Empty;
            });
            string bundlePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "node_modules");
            string probePaths = $"{solutionPath}{bundlePath}";

            var startInfo = new ProcessStartInfo
            {
                FileName = "node.exe",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                Arguments =
                    //"--inspect-brk=9222 " +
                    $"\"{Path.Combine(bundlePath, "@angular", "language-server", "index.js")}\"" +
                    " --logVerbosity verbose" +
                    " --logToConsole" +
                    " --stdio" +
                    $" --tsProbeLocations \"{probePaths}\"" +
                    $" --ngProbeLocations \"{probePaths}\""
            };

            var process = new Process { StartInfo = startInfo };

            if (process.Start())
            {
                return new Connection(process.StandardOutput.BaseStream, process.StandardInput.BaseStream);
            }

            return null;
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
