namespace AngularLanguageService.Shared.LanguageServer
{
    /// <summary>
    /// Constants of the VS Angular Language Server.
    /// </summary>
    internal static class AngularConstants
    {
        internal const string TypeScriptContentTypeName = "TypeScript";

        internal const string AngularComponentContentTypeName = "angularcomponent";
        internal const string AngularComponentContentTypeExtension = ".component.html";

        internal const string ImportingConstructorMessage = "This exported object must be obtained through the MEF export provider.";
    }
}
