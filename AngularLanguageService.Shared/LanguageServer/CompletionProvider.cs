using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Microsoft.WebTools.Languages.Html.Editor.Completion;
using Microsoft.WebTools.Languages.Html.Editor.Completion.Def;
using Newtonsoft.Json.Linq;

namespace AngularLanguageService.Shared.LanguageServer
{ 
	[HtmlCompletionProvider("Children", "*")]
	[ContentType(ContentDefinitions.AngularComponentContentTypeName)]
	internal sealed class CompletionProvider : IHtmlCompletionListProvider
	{
		private readonly ILanguageServiceBroker2 languageServiceBroker;
		private readonly LanguageClient languageClient;

		[ImportingConstructor]
		internal CompletionProvider(ILanguageServiceBroker2 languageServiceBroker, [Import(LanguageClient.AngularLanguageClientName, typeof(ILanguageClient))] LanguageClient languageClient)
		{
			this.languageServiceBroker = languageServiceBroker;
			this.languageClient = languageClient;
		}

		#region IHtmlCompletionListProvider implementation
		IList<HtmlCompletion> IHtmlCompletionListProvider.GetEntries(HtmlCompletionContext context)
		{
			var completions = new List<HtmlCompletion>();

#pragma warning disable VSTHRD104 // Offer async methods
			var angularCompletions = ThreadHelper.JoinableTaskFactory.Run(async () => await GetAngularCompletionsAsync(context));
#pragma warning restore VSTHRD104 // Offer async methods

			if (angularCompletions is not null)
			{
				foreach (CompletionItem completion in angularCompletions)
				{
					var htmlCompletion = new HtmlCompletion(
						displayText: completion.Label,
						insertionText: completion.TextEdit?.NewText ?? completion.Label,
						description: completion.Detail ?? string.Empty,
						iconSource: null,
						iconAutomationText: string.Empty,
						session: context.Session
					);
					completions.Add(htmlCompletion);
				}
			}

			return completions;
		}
		#endregion

		private async Task<CompletionItem[]?> GetAngularCompletionsAsync(HtmlCompletionContext context)
		{
			ITextView textView = context!.Session!.TextView;
			int position = textView.Caret.Position.BufferPosition.Position;
			context!.Element!.Root.TextProvider.GetLineAndColumnFromPosition(position, out int line, out int column);

			var completionParams = new CompletionParams()
			{
				TextDocument = new TextDocumentIdentifier { Uri = new Uri(context!.Document!.Url.AbsolutePath) },
				Position = new Position { Line = line, Character = column }
			};

#pragma warning disable CS0618 // Type or member is obsolete
			var resultToken = await languageServiceBroker.RequestAsync(
				languageClient, 
				Methods.TextDocumentCompletionName, 
				JToken.FromObject(completionParams), 
				CancellationToken.None
			);
#pragma warning restore CS0618 // Type or member is obsolete

			return resultToken?.ToObject<CompletionItem[]>();
		}
	}
}