using System.ComponentModel.Composition;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Utilities;

namespace AngularLanguageService.LanguageServer
{
	internal static class ContentDefinitions
	{
		internal const string TypeScriptContentTypeName = "TypeScript";

		internal const string AngularComponentContentTypeName = "angularcomponent";
		internal const string AngularComponentContentTypeExtension = ".component.html";

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS0649 // Field 'field' is never assigned to, and will always have its default value null.
		[Export]
		[Name(AngularComponentContentTypeName)]
		[BaseDefinition(CodeRemoteContentDefinition.CodeRemoteContentTypeName)]
		[BaseDefinition("html")]
		[BaseDefinition("htmlx")]
		internal static ContentTypeDefinition AngularComponentContentTypeDefinition;

		[Export]
		[FileExtension(AngularComponentContentTypeExtension)]
		[ContentType(AngularComponentContentTypeName)]
		internal static FileExtensionToContentTypeDefinition AngularComponentFileExtensionDefinition;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CS0649 // Field 'field' is never assigned to, and will always have its default value null.
	}
}