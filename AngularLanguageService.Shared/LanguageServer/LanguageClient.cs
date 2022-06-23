using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;

namespace AngularLanguageService.Shared.LanguageServer
{
	[Export(typeof(ILanguageClient))]
	[ContentType(ContentDefinitions.AngularComponentContentTypeName)]
	[ContentType(ContentDefinitions.TypeScriptContentTypeName)]
	internal sealed class LanguageClient : ILanguageClient
	{
		internal const string AngularLanguageClientName = "Angular Language Service Extension";

		private static readonly string[] ConfigurationFiles = new string[] { "**/tsconfig.json" };

		#region ILanguageClient implementation
		public event AsyncEventHandler<EventArgs>? /*ILanguageClient*/ StartAsync;

		public event AsyncEventHandler<EventArgs>? /*ILanguageClient*/ StopAsync;

		string ILanguageClient.Name => AngularLanguageClientName;

		IEnumerable<string>? ILanguageClient.ConfigurationSections => null;

		object? ILanguageClient.InitializationOptions => null;

		IEnumerable<string>? ILanguageClient.FilesToWatch => ConfigurationFiles;

		bool ILanguageClient.ShowNotificationOnInitializeFailed => true;

		Task<Connection?> ILanguageClient.ActivateAsync(CancellationToken token)
		{
			throw new NotImplementedException();
		}

		Task ILanguageClient.OnLoadedAsync()
		{
			throw new NotImplementedException();
		}

		Task ILanguageClient.OnServerInitializedAsync() => Task.CompletedTask;

		Task<InitializationFailureContext?> ILanguageClient.OnServerInitializeFailedAsync(ILanguageClientInitializationInfo initializationState)
		{
			var failureContext = new InitializationFailureContext { FailureMessage = initializationState.InitializationException!.Message };
			return Task.FromResult(failureContext)!;
		}
		#endregion
	}
}
