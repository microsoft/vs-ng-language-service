using System.ComponentModel.Composition;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Utilities;

namespace AngularLanguageService.Shared.LanguageServer
{
	internal static class ContentDefinitions
	{
		internal const string AngularComponentContentTypeName = "angularcomponent";
		internal const string AngularComponentContentTypeExtension = ".component.html";
		internal const string TypeScriptContentTypeName = "typescript";
		internal const string TypeScriptContentTypeExtension = ".ts";

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS0649 // Field 'field' is never assigned to, and will always have its default value null.
		[Export]
		[Name(AngularComponentContentTypeName)]
		[BaseDefinition(CodeRemoteContentDefinition.CodeRemoteContentTypeName)]
		[BaseDefinition("html")]
		internal static ContentTypeDefinition AngularComponentContentTypeDefinition;

		[Export]
		[FileExtension(AngularComponentContentTypeExtension)]
		[ContentType(AngularComponentContentTypeName)]
		internal static FileExtensionToContentTypeDefinition AngularComponentFileExtensionDefinition;

		[Export]
		[Name(TypeScriptContentTypeName)]
		[BaseDefinition(CodeRemoteContentDefinition.CodeRemoteContentTypeName)]
		internal static ContentTypeDefinition TypeScriptContentTypeDefinition;

		[Export]
		[FileExtension(TypeScriptContentTypeExtension)]
		[ContentType(TypeScriptContentTypeName)]
		internal static FileExtensionToContentTypeDefinition TypeScriptFileExtensionDefinition;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CS0649 // Field 'field' is never assigned to, and will always have its default value null.
	}
}