using System.Collections.Generic;
using Microsoft.WebTools.Languages.Html.Editor.Completion;
using Microsoft.WebTools.Languages.Html.Editor.Completion.Def;

namespace AngularLanguageService.Html.Completion
{
    /// <summary>
    /// An interface that supplies list of entries to intellisense.
    /// There may be more than one provider.
    /// Providers are exported via MEF, <see cref="HtmlCompletionProviderAttribute"/>
    /// </summary>
    public interface IHtmlCompletionListProvider
    {
        /// <summary>
        /// Retrieves list of intellisense entries
        /// </summary>
        /// <param name="context">Completion context</param>
        /// <returns>List of completion entries</returns>
        IList<HtmlCompletion> GetEntries(HtmlCompletionContext context);
    }
}
