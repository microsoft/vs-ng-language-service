using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using AngularLanguageService.Shared.LanguageServer;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Microsoft.WebTools.Languages.Html.Editor.Completion;
using Microsoft.WebTools.Languages.Html.Editor.Completion.Def;

namespace AngularLanguageService.LanguageServer
{
    /// <summary>
    /// <see cref="IHtmlCompletionListProvider"/> for adding Angular completions to <c>.component.html</c> files
    /// (without losing the built-in HTML completions).
    /// </summary>
    [HtmlCompletionProvider("Children", "*")]
    [ContentType(AngularConstants.AngularComponentContentTypeName)]
    internal sealed class CompletionProvider : IHtmlCompletionListProvider
    {
        private readonly LanguageClient languageClient;

        [ImportingConstructor]
        [Obsolete(AngularConstants.ImportingConstructorMessage, error: true)]
        internal CompletionProvider([Import(LanguageClient.AngularLanguageClientName, typeof(ILanguageClient))] LanguageClient languageClient)
        {
            this.languageClient = languageClient;
        }

        #region IHtmlCompletionListProvider implementation
        IList<HtmlCompletion> IHtmlCompletionListProvider.GetEntries(HtmlCompletionContext context)
        {
            var completions = new List<HtmlCompletion>();

#pragma warning disable VSTHRD104 // Offer async methods
            CompletionItem[] angularCompletions = ThreadHelper.JoinableTaskFactory.Run(async () => await GetAngularCompletionsAsync(context));
#pragma warning restore VSTHRD104 // Offer async methods

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

            return completions;
        }
        #endregion

        private Task<CompletionItem[]> GetAngularCompletionsAsync(HtmlCompletionContext context)
        {
            ITextView textView = context.Session.TextView;
            int position = textView.Caret.Position.BufferPosition.Position;
            context.Element.Root.TextProvider.GetLineAndColumnFromPosition(position, out int line, out int column);

            var uri = new Uri(context.Document.Url.AbsolutePath);
            var completionParams = new CompletionParams
            {
                TextDocument = new TextDocumentIdentifier { Uri = uri },
                Position = new Position { Line = line, Character = column }
            };

            return languageClient.GetAngularCompletionsAsync(completionParams);
        }
    }
}