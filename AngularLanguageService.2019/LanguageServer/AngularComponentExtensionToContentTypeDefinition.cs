using System.ComponentModel.Composition;
using AngularLanguageService.Shared.LanguageServer;
using Microsoft.VisualStudio.Utilities;

namespace AngularLanguageService.LanguageServer
{
    /// <summary>
    /// Maps <c>.component.html</c> files to belong to the <see cref="AngularConstants.AngularComponentContentTypeName">
    /// Angular component content type</see>.
    /// </summary>
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
