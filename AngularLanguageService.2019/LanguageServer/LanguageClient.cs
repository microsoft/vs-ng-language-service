using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using StreamJsonRpc;

namespace AngularLanguageService.LanguageServer
{
	[Export(typeof(ILanguageClient))]
	[Export(AngularLanguageClientName, typeof(ILanguageClient))]
	[ContentType(ContentDefinitions.AngularComponentContentTypeName)]
	[ContentType(ContentDefinitions.TypeScriptContentTypeName)]
	internal sealed class LanguageClient : ILanguageClient, ILanguageClientCustomMessage2
	{
		internal const string AngularLanguageClientName = "Angular Language Service Extension";

		/// <summary>
		/// Pane in the output window for logging the LSP communication.
		/// </summary>
		// TODO: Control the verbosity of this pane?
		internal OutputWindowPane OutputPane = null!; // The pane is created in ActivateAsync, so it's never actually null when we use it.

		private static readonly string[] ConfigurationFiles = new string[] { "**/tsconfig.json" };

		private readonly MiddleLayer middleLayer;
		private JsonRpc? customMessageRpc;

		[ImportingConstructor]
		internal LanguageClient(MiddleLayer middleLayer)
		{
			this.middleLayer = middleLayer;
		}

		#region ILanguageClient implementation
		public event AsyncEventHandler<EventArgs>? /*ILanguageClient*/ StartAsync;

#pragma warning disable CS0067 // The event 'LanguageClient.StopAsync' is never used.
		public event AsyncEventHandler<EventArgs>? /*ILanguageClient*/ StopAsync;
#pragma warning restore CS0067 // The event 'LanguageClient.StopAsync' is never used.

		string ILanguageClient.Name => AngularLanguageClientName;

		IEnumerable<string>? ILanguageClient.ConfigurationSections => null;

		object? ILanguageClient.InitializationOptions => null;

		IEnumerable<string>? ILanguageClient.FilesToWatch => ConfigurationFiles;

		async Task<Connection?> ILanguageClient.ActivateAsync(CancellationToken token)
		{
			OutputPane = await OutputWindowPane.CreateAsync(AngularLanguageClientName);

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
				return new Connection(process.StandardOutput.BaseStream, process.StandardInput.BaseStream);
			}

			return null;
		}

		Task ILanguageClient.OnLoadedAsync() => StartAsync.InvokeAsync(this, EventArgs.Empty);

		Task ILanguageClient.OnServerInitializedAsync() => Task.CompletedTask;

		Task ILanguageClient.OnServerInitializeFailedAsync(Exception exception) => Task.CompletedTask;
		#endregion

		#region ILanguageClientCustomMessage2 implementation
		object ILanguageClientCustomMessage2.MiddleLayer => middleLayer;

		object? ILanguageClientCustomMessage2.CustomMessageTarget => null;

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
