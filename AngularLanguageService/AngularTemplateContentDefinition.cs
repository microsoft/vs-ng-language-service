using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace AngularLanguageService
{
    public static class AngularTemplateContentDefinition
    {
        public const string Name = "angularTemplate";

        [Export]
        [Name(Name)]
        [BaseDefinition(CodeRemoteContentDefinition.CodeRemoteBaseTypeName)]
        [BaseDefinition("HTMLX")]
        internal static ContentTypeDefinition BarContentTypeDefinition;

        [Export]
        [FileExtension(".component.html")]
        [ContentType(Name)]
        internal static FileExtensionToContentTypeDefinition BarFileExtensionDefinition;
    }
}