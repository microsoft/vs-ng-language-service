using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using Newtonsoft.Json.Linq;

namespace AngularLanguageService.LanguageServer
{
    /// <summary>
    /// <see cref="ILanguageClientMiddleLayer"/> of the VS 2019 Angular Language Service extension, whose main
    /// purpose is blocking the <see cref="Methods.TextDocumentCompletion">completion LSP request</see> so that
    /// we can use the WebTools' shim instead.
    /// </summary>
    [Export]
    internal sealed class MiddleLayer : ILanguageClientMiddleLayer
    {
        #region ILanguageClientMiddleLayer
        bool ILanguageClientMiddleLayer.CanHandle(string methodName) => true;

        Task ILanguageClientMiddleLayer.HandleNotificationAsync(string methodName, JToken methodParam, Func<JToken, Task> sendNotification)
            => sendNotification(methodParam);

        Task<JToken> ILanguageClientMiddleLayer.HandleRequestAsync(string methodName, JToken methodParam, Func<JToken, Task<JToken>> sendRequest)
        {
            if (string.Equals(methodName, Methods.TextDocumentCompletionName))
            {
                return Task.FromResult<JToken>(null);
            }
            else
            {
                return sendRequest(methodParam);
            }
        }
        #endregion
    }
}
