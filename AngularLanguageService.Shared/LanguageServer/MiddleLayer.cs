using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using Newtonsoft.Json.Linq;

namespace AngularLanguageService.LanguageServer
{
	[Export]
	internal sealed class MiddleLayer : ILanguageClientMiddleLayer
	{
		private readonly Lazy<LanguageClient> languageClient;

		[ImportingConstructor]
		internal MiddleLayer([Import(LanguageClient.AngularLanguageClientName, typeof(ILanguageClient))] Lazy<LanguageClient> languageClient)
		{
			this.languageClient = languageClient;
		}

		#region ILanguageClientMiddleLayer
		bool ILanguageClientMiddleLayer.CanHandle(string methodName) => true;

		Task ILanguageClientMiddleLayer.HandleNotificationAsync(string methodName, JToken methodParam, Func<JToken, Task> sendNotification)
		{
			if (string.Equals(methodName, Methods.WindowLogMessageName))
			{
				var logParams = methodParam.ToObject<LogMessageParams>()!;
				return HandleWindowLogNotificationAsync(logParams);
			}
			else
			{
				return sendNotification(methodParam);
			}
		}

		Task<JToken?> ILanguageClientMiddleLayer.HandleRequestAsync(string methodName, JToken methodParam, Func<JToken, Task<JToken?>> sendRequest)
		{
			if (string.Equals(methodName, Methods.TextDocumentCompletionName))
			{
				return Task.FromResult<JToken?>(null);
			}
			else
			{
				return sendRequest(methodParam);
			}
		}
		#endregion

		private Task HandleWindowLogNotificationAsync(LogMessageParams logParams)
		{
			string message = $"{Enum.GetName(typeof(MessageType), logParams.MessageType)}: {logParams.Message}";
			return languageClient.Value.OutputPane.WriteLineAsync(message);
		}
	}
}
