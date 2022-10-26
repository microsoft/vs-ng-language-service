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
