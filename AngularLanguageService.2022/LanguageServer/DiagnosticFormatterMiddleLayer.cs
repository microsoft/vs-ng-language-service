using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using AngularLanguageService.Shared.LanguageServer;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using Newtonsoft.Json.Linq;

namespace AngularLanguageService.LanguageServer
{
    [Export(typeof(MiddleLayerHandler))]
    internal sealed class DiagnosticFormatterMiddleLayer : MiddleLayerHandler
    {
        [ImportingConstructor]
        [Obsolete(AngularConstants.ImportingConstructorMessage, error: true)]
        public DiagnosticFormatterMiddleLayer() : base(methodsToHandle: Methods.TextDocumentPublishDiagnosticsName)
        {
        }

        public override Task HandleNotificationAsync(string methodName, JToken methodParam, Func<JToken, Task> sendNotification)
        {
            // TODO: Format diagnostics (see #47).
            return base.HandleNotificationAsync(methodName, methodParam, sendNotification);
        }
    }
}
