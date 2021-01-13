using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServer.Client;
using LSP = Microsoft.VisualStudio.LanguageServer.Protocol;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Shell;
using Microsoft.WebTools.Languages.Html.Editor.Completion;
using Microsoft.WebTools.Languages.Html.Editor.Completion.Def;
using Microsoft.WebTools.Languages.Shared.ContentTypes;
using Newtonsoft.Json.Linq;

namespace AngularLanguageService
{
    [Export(typeof(IHtmlCompletionListProvider))]
    [HtmlCompletionProvider("Children", "*")]
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
                JToken answer = await CallLanguageServiceBrokerAsync(context);
                return answer;
            });

            var entry = new HtmlCompletion(angularCompletions.ToString(), angularCompletions.ToString(), String.Empty, null, null, context.Session);

            list.Add(entry);
            return list;
        }

        private async Task<JToken> CallLanguageServiceBrokerAsync(HtmlCompletionContext context)
        {
            LSP.CompletionParams completionParams = new LSP.CompletionParams();
            completionParams.TextDocument = new LSP.TextDocumentIdentifier() { Uri = new Uri(context.Document.Url.Name) };
            completionParams.Position = context.Position;
            completionParams.Context = new LSP.CompletionContext() { TriggerKind = LSP.CompletionTriggerKind.Invoked };
            JToken requestParams = JObject.FromObject(completionParams);
            await this.outputPane.WriteAsync($"[HtmlCompletionProvider -> AngularLanguageClient] Request: {requestParams.ToString()}");

            string[] contentType = new string[] { AngularTemplateContentDefinition.Name };
            CancellationTokenSource source = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
            var result = await this.languageServiceBroker.RequestAsync(contentType, null, "textDocument/completion", requestParams, source.Token);
            //validate language client
            return result.Item2;
        }
    }
}
