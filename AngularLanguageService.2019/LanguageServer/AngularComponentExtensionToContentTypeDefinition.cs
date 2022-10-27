using System.ComponentModel.Composition;
using AngularLanguageService.Shared.LanguageServer;
using Microsoft.VisualStudio.Utilities;

namespace AngularLanguageService.LanguageServer
{
    internal static class AngularComponentExtensionToContentTypeDefinition
    {
#pragma warning disable CS0649 // Field 'AngularComponentFileExtensionDefinition' is never assigned to, and will always have its default value null.
        [Export]
        [ContentType(AngularConstants.AngularComponentContentTypeName)]
        [FileExtension(AngularConstants.AngularComponentContentTypeExtension)]
        internal static FileExtensionToContentTypeDefinition AngularComponentFileExtensionDefinition;
#pragma warning restore CS0649 // Field 'AngularComponentFileExtensionDefinition' is never assigned to, and will always have its default value null.
    }
}
