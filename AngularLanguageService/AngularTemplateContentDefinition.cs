// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Utilities;

namespace AngularLanguageService
{
    public static class AngularTemplateContentDefinition
    {
        public const string Name = "angularTemplate";

        [Export]
        [Name(Name)]
        [BaseDefinition(CodeRemoteContentDefinition.CodeRemoteBaseTypeName)]
        [BaseDefinition("HTMLX")]
        internal static ContentTypeDefinition AngularTemplateContentTypeDefinition;

        [Export]
        [FileExtension(".component.html")]
        [ContentType(Name)]
        internal static FileExtensionToContentTypeDefinition AngularTemplateFileExtensionDefinition;
    }
}