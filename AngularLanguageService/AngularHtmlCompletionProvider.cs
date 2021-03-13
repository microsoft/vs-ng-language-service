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
using Newtonsoft.Json.Linq;
using System.Linq;

namespace AngularLanguageService
{
    [HtmlCompletionProvider("Children", "*")]
    [ContentType(AngularTemplateContentDefinition.Name)]
    public class AngularHtmlCompletionProvider : IHtmlCompletionListProvider
    {
        [Import]
        private readonly ILanguageClientBroker broker = null;

        public IList<HtmlCompletion> GetEntries(HtmlCompletionContext context)
        {
            var list = new List<HtmlCompletion>();

            var angularCompletions = ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                JToken completions = await CallLanguageServiceBrokerAsync(context).ConfigureAwait(false);
                return completions;
            });

            if (angularCompletions != null)
            {
                JToken[] tokenizedCompletions = angularCompletions.ToObject<JToken[]>();
                foreach (JToken completion in tokenizedCompletions)
                {
                    string label = completion["label"].ToObject<string>();
                    list.Add(new HtmlCompletion(label, label, String.Empty, null, null, context.Session));
                }
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
            };

            var requestParams = JObject.FromObject(completionParams);
            var ct = new CancellationTokenSource(200);

            var assembly = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name == "Microsoft.VisualStudio.LanguageServer.Client.Implementation").First();
            var t = assembly.GetType("Microsoft.VisualStudio.LanguageServer.Client.ILanguageServiceBroker2");
            var m = t.GetMethod("RequestAsync", new[] { typeof(string[]), typeof(Func<JToken, bool>), typeof(string), typeof(JToken), typeof(CancellationToken) });
            var r = (Task<(ILanguageClient, JToken)>)m.Invoke(broker, new object[] { new[] { AngularTemplateContentDefinition.Name }, null, "textDocument/completion", requestParams, ct.Token });

            var result = await r.ConfigureAwait(false);
            return result.Item2;
        }
    }
}
