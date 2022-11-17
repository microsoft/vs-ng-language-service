using System.ComponentModel.Composition;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Utilities;
using Microsoft.WebTools.Languages.Shared.ContentTypes;

namespace AngularLanguageService.Shared.LanguageServer
{
    /// <summary>
    /// Content type definition of <c>.component.html</c> Angular files.
    /// </summary>
    internal static class AngularComponentContentTypeDefinition
    {
#pragma warning disable CS0649 // Field 'field' is never assigned to, and will always have its default value null.
        [Export]
        [Name(AngularConstants.AngularComponentContentTypeName)]
#if VS2019
        [BaseDefinition(HtmlContentTypeDefinition.HtmlContentType)]
#elif VS2022
        [BaseDefinition(HtmlContentTypeDefinition.HtmlDelegationContentType)]
#endif
        [BaseDefinition(CodeRemoteContentDefinition.CodeRemoteContentTypeName)]
        internal static ContentTypeDefinition AngularComponentContentType;
#pragma warning restore CS0649 // Field 'field' is never assigned to, and will always have its default value null.
    }
}
