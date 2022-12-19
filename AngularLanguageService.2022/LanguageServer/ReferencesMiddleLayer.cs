using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using AngularLanguageService.Shared.LanguageServer;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using Newtonsoft.Json.Linq;

namespace AngularLanguageService.LanguageServer
{
    [Export(typeof(MiddleLayerHandler))]
    internal sealed class ReferencesMiddleLayer : MiddleLayerHandler
    {
        [ImportingConstructor]
        [Obsolete(AngularConstants.ImportingConstructorMessage, error: true)]
        public ReferencesMiddleLayer() : base(methodsToHandle: Methods.TextDocumentReferencesName)
        {
        }

        public override async Task<JToken> HandleRequestAsync(string methodName, JToken methodParam, Func<JToken, Task<JToken>> sendRequest)
        {
            var response = (await sendRequest(methodParam)).ToObject<Location[]>();

            foreach (Location location in response)
            {
                // For the links to these references to work, we need to unescape the URIs in the response
                // and remove leading slashes from the path.
                // Refer to https://github.com/microsoft/vscode/issues/169453 for extra context.
                string uriString = Uri.UnescapeDataString(location.Uri.LocalPath).TrimStart('/');
                location.Uri = new Uri(uriString);
            }

            return JToken.FromObject(response);
        }
    }
}
