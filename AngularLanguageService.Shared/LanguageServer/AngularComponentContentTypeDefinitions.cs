using System.ComponentModel.Composition;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Utilities;
using Microsoft.WebTools.Languages.Shared.ContentTypes;

namespace AngularLanguageService.Shared.LanguageServer
{
    /// <summary>
    /// Content type definitions of <c>.component.html</c> Angular files.
    /// </summary>
    internal static class AngularComponentContentTypeDefinitions
    {
#pragma warning disable CS0649 // Field 'field' is never assigned to, and will always have its default value null.
        [Export]
        [Name(AngularConstants.AngularComponentContentTypeName)]
        [BaseDefinition(HtmlContentTypeDefinition.HtmlContentType)]
        [BaseDefinition(CodeRemoteContentDefinition.CodeRemoteContentTypeName)]
        internal static ContentTypeDefinition AngularComponentContentType;

        [Export]
        [Name(AngularConstants.AngularComponentDelegationContentTypeName)]
        [BaseDefinition(HtmlContentTypeDefinition.HtmlDelegationContentType)]
        [BaseDefinition(CodeRemoteContentDefinition.CodeRemoteContentTypeName)]
        internal static ContentTypeDefinition AngularComponentDelegationContentType;
    }
}
