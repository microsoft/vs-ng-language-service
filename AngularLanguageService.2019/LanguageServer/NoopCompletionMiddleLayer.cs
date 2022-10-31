using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using AngularLanguageService.Shared.LanguageServer;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using Newtonsoft.Json.Linq;

namespace AngularLanguageService.LanguageServer
{
    /// <summary>
    /// <see cref="MiddleLayerHandler"/> whose main purpose is blocking the <see cref="Methods.TextDocumentCompletion">completion LSP request</see>
    /// so that we can use the WebTools' shim instead.
    /// </summary>
    [Export(typeof(MiddleLayerHandler))]
    internal sealed class NoopCompletionMiddleLayer : MiddleLayerHandler
    {
        [ImportingConstructor]
        [Obsolete(AngularConstants.ImportingConstructorMessage, error: true)]
        public NoopCompletionMiddleLayer() : base(methodsToHandle: Methods.TextDocumentCompletionName)
        {
        }

        public override Task<JToken> HandleRequestAsync(string methodName, JToken methodParam, Func<JToken, Task<JToken>> sendRequest) => Task.FromResult<JToken>(null);
    }
}
