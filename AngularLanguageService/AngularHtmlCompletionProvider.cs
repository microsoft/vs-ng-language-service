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
    //These two are unnecessary
    //[Export(typeof(IHtmlCompletionListProvider))]
    //[ContentType(HtmlContentTypeDefinition.HtmlContentType)]

    [HtmlCompletionProvider("Children", "*")]
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
                JToken completions = await CallLanguageServiceBrokerAsync(context);
                return completions;
            });

            JToken[] tokenizedCompletions = angularCompletions.ToObject<JToken[]>();
            foreach (JToken completion in tokenizedCompletions)
            {
                string label = completion["label"].ToObject<string>();
                list.Add(new HtmlCompletion(label, label, String.Empty, null, null, context.Session));
            }

            return list;
        }

        private async Task<JToken> CallLanguageServiceBrokerAsync(HtmlCompletionContext context)
        {
            var textView = context.Session.TextView;
            var position = textView.Caret.Position.BufferPosition.Position;
            context.Element.Root.TextProvider.GetLineAndColumnFromPosition(position, out int lineNum, out int colNum);

            var completionParams = new LSP.CompletionParams()
            {
                TextDocument = new LSP.TextDocumentIdentifier() { Uri = new Uri(context.Document.Url.AbsolutePath) },
                Position = new LSP.Position(lineNum, colNum),
                //Context = new LSP.CompletionContext() { TriggerKind = LSP.CompletionTriggerKind.Invoked }
            };

            // TEST STRING
            //string json = @"{
            //    textDocument: { uri: 'file:///C:/Users/sheya/source/repos/DotNet5Angular/DotNet5Angular/ClientApp/src/app/counter/counter.component.html' },
            //    position: { line: 10, character: 1 }
            //    context: { triggerKind: 1 }
            //}";
            //var requestParams = JObject.Parse(json);

            var requestParams = JObject.FromObject(completionParams);
            await this.outputPane.WriteAsync($"[HtmlCompletionProvider -> AngularLanguageClient] Request: {requestParams}");

            string[] contentType = new string[] { AngularTemplateContentDefinition.Name };
            CancellationTokenSource source = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));

            var result = await this.languageServiceBroker.RequestAsync(contentType, null, "textDocument/completion", requestParams, CancellationToken.None);
            return result.Item2;
        }
    }
}
