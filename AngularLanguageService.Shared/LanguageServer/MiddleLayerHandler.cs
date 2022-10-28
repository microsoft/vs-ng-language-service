using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServer.Client;
using Newtonsoft.Json.Linq;

namespace AngularLanguageService.Shared.LanguageServer
{
    /// <summary>
    /// Base class for an <see cref="ILanguageClientMiddleLayer"/> of the Angular LSP service.
    /// </summary>
    /// <remarks>
    /// <see cref="AggregatingMiddleLayer"/> delegates the concrete middle layer handlers and delegates the respective 
    /// requests/notifications to the correct handler.
    /// </remarks>
    internal abstract class MiddleLayerHandler : ILanguageClientMiddleLayer
    {
        private readonly HashSet<string> methodsToHandle;

        protected MiddleLayerHandler(params string[] methodsToHandle)
        {
            this.methodsToHandle = new HashSet<string>(methodsToHandle);
        }

        public bool CanHandle(string methodName) => this.methodsToHandle.Contains(methodName);

        public virtual Task HandleNotificationAsync(string methodName, JToken methodParam, Func<JToken, Task> sendNotification) => sendNotification(methodParam);

        public virtual Task<JToken> HandleRequestAsync(string methodName, JToken methodParam, Func<JToken, Task<JToken>> sendRequest) => sendRequest(methodParam);
    }
}

