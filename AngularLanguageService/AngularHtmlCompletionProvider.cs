using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServer.Client;
using LSP = Microsoft.VisualStudio.LanguageServer.Protocol;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.WebTools.Languages.Html;
using Microsoft.WebTools.Languages.Html.Editor.Completion;
using Microsoft.WebTools.Languages.Html.Editor.Completion.Def;
using Microsoft.WebTools.Languages.Html.Editor.Document;
using Microsoft.WebTools.Languages.Shared.ContentTypes;
using Microsoft.WebTools.Languages.Shared.Editor;
using Newtonsoft.Json.Linq;

namespace AngularLanguageService
{
    [Export(typeof(IHtmlCompletionListProvider))]
    [ContentType(HtmlContentTypeDefinition.HtmlContentType)]
    [ContentType(AngularTemplateContentDefinition.Name)]
    public class AngularHtmlCompletionProvider : IHtmlCompletionListProvider
    {
        [Import]
        private readonly ILanguageServiceBroker2 languageServiceBroker = null;

        [Import]
        private readonly AngularLanguageServiceOutputPane outputPane = null;

        public IList<HtmlCompletion> GetEntries(HtmlCompletionContext context)
        {
            var list = new List<HtmlCompletion>();

            var angularCompletions = ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                //(ILanguageClient, JToken) result = await CallLanguageServiceBrokerAsync(context);
                JToken answer = await CallLanguageServiceBrokerAsync(context);
                return answer;
            });

            foreach (string completion in angularCompletions.ToObject<string[]>())
            {
                list.Add(new HtmlCompletion(completion, completion, String.Empty, null, null, context.Session));
            }

            return list;
        }

        private async Task<JToken> CallLanguageServiceBrokerAsync(HtmlCompletionContext context)
        {
            ITextView textView = context.Session.TextView;
            int position = textView.Caret.Position.BufferPosition.Position;
            context.Element.Root.TextProvider.GetLineAndColumnFromPosition(position, out int lineNum, out int colNum);

            var completionParams = new LSP.CompletionParams()
            {
                TextDocument = new LSP.TextDocumentIdentifier() { Uri = new Uri(context.Document.Url.AbsolutePath) },
                Position = new LSP.Position(lineNum, colNum),
                Context = new LSP.CompletionContext() { TriggerKind = LSP.CompletionTriggerKind.Invoked, TriggerCharacter = null }   
            };

            JToken requestParams = JObject.FromObject(completionParams);
            string requestJson = requestParams.ToString();
            await this.outputPane.WriteAsync($"[HtmlCompletionProvider -> AngularLanguageClient] Request: {requestJson}");

            string[] contentType = new string[] { AngularTemplateContentDefinition.Name };
            CancellationTokenSource source = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));

            var result = await this.languageServiceBroker.RequestAsync(contentType, null, "textDocument/completion", requestParams, CancellationToken.None);
            return result.Item2;
        }
    }
}
