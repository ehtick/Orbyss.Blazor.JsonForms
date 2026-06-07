using Orbyss.Blazor.JsonForms.Core.Models;

namespace Orbyss.Blazor.JsonForms.Core.Context.Interfaces;

/// <summary>
/// Provides renderer or application default labels that are merged into a form's translation
/// schema when the user schema does not define those keys.
/// </summary>
public interface IJsonFormDefaultTranslationProvider
{
    /// <summary>Gets default translation sections grouped by language code.</summary>
    DefaultTranslationResourcesDictionary GetDefaultTranslations();
}
