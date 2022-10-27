using System.ComponentModel.Composition;
using AngularLanguageService.Shared.LanguageServer;
using Microsoft.VisualStudio.Utilities;
using Microsoft.WebTools.Languages.Shared.ContentTypes;

namespace AngularLanguageService.LanguageServer
{
    /// <summary>
    /// Maps <c>.component.html</c> files to belong to the <see cref="AngularConstants.AngularComponentDelegationContentTypeName">
    /// Angular component content type</see>.
    /// </summary>
    /// <remarks>
    /// Because <c>.html</c> is a suffix of the <c>.component.html</c> extension, the <see cref="OrderAttribute"/> ensures that
    /// we correctly identify Angular component files.
    /// </remarks>
    [Export(typeof(IFilePathToContentTypeProvider))]
    [Name(nameof(AngularComponentToContentTypeProvider))]
    [FileExtension(AngularConstants.AngularComponentContentTypeExtension)]
    [Order(Before = HtmlContentTypeDefinition.HtmlDelegationContentType)]
    internal sealed class AngularComponentToContentTypeProvider : IFilePathToContentTypeProvider
    {
        private readonly IContentTypeRegistryService contentTypeRegistryService;

        [ImportingConstructor]
        public AngularComponentToContentTypeProvider(IContentTypeRegistryService contentTypeRegistryService)
        {
            this.contentTypeRegistryService = contentTypeRegistryService;
        }

        bool IFilePathToContentTypeProvider.TryGetContentTypeForFilePath(string filePath, out IContentType contentType)
        {
            contentType = this.contentTypeRegistryService.GetContentType(AngularConstants.AngularComponentDelegationContentTypeName);
            return true;
        }
    }
}
