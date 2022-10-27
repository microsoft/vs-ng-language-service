using System.ComponentModel.Composition;
using AngularLanguageService.Shared.LanguageServer;
using Microsoft.VisualStudio.Utilities;
using Microsoft.WebTools.Languages.Shared.ContentTypes;

namespace AngularLanguageService.LanguageServer
{
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
