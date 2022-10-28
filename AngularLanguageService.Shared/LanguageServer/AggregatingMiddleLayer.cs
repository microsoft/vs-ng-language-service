using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServer.Client;
using Newtonsoft.Json.Linq;

namespace AngularLanguageService.Shared.LanguageServer
{
    /// <summary>
    /// Composite <see href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.languageserver.client.ilanguageclientmiddlelayer?view=visualstudiosdk-2022">
    /// middle layer</see> which aggregates all concrete instances of <see cref="MiddleLayerHandler"/> and 
    /// accordingly delegates the interception to the aggregated layers.
    /// </summary>
    /// <remarks>
    /// Because a single handler will be invoked for each request or notification, assumes that there's at most 
    /// one middle layer handling any given method.
    /// </remarks>
    [Export]
    internal sealed class AggregatingMiddleLayer : ILanguageClientMiddleLayer
    {
        private readonly IEnumerable<MiddleLayerHandler> middleLayerHandlers;

        [ImportingConstructor]
        public AggregatingMiddleLayer([ImportMany] IEnumerable<MiddleLayerHandler> middleLayerHandlers)
        {
            this.middleLayerHandlers = middleLayerHandlers;
        }

        bool ILanguageClientMiddleLayer.CanHandle(string methodName) => this.middleLayerHandlers.Any(handler => handler.CanHandle(methodName));

        Task ILanguageClientMiddleLayer.HandleNotificationAsync(string methodName, JToken methodParam, Func<JToken, Task> sendNotification)
            => this.GetHandler(methodName).HandleNotificationAsync(methodName, methodParam, sendNotification);

        Task<JToken> ILanguageClientMiddleLayer.HandleRequestAsync(string methodName, JToken methodParam, Func<JToken, Task<JToken>> sendRequest)
            => this.GetHandler(methodName).HandleRequestAsync(methodName, methodParam, sendRequest);

        private ILanguageClientMiddleLayer GetHandler(string methodName) => this.middleLayerHandlers.First(layer => layer.CanHandle(methodName));
    }
}
