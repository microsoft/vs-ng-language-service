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
            var diagnosticParams = methodParam.ToObject<PublishDiagnosticParams>();

            foreach (Diagnostic diagnostic in diagnosticParams.Diagnostics)
            {
                // Add an Angular code prefix
                diagnostic.Code = "(NG) " + diagnostic.Code?.Value;

                // The Angular parser adds location information to errors (see https://github.com/angular/angular/blob/37ba6104498202b671f5a5a6bbfacc4df501788b/packages/compiler/src/expression_parser/ast.ts#L12-L18),
                // resulting in redundant messages in the error list.
                if (diagnostic.Message.StartsWith("Parser Error: "))
                {
                    int endOfMessage = diagnostic.Message.LastIndexOf("] in ");
                    diagnostic.Message = diagnostic.Message.Substring(0, endOfMessage >= 0 ? endOfMessage + 1 : diagnostic.Message.Length);
                }
            }

            return sendNotification(JToken.FromObject(diagnosticParams));
        }
    }
}
