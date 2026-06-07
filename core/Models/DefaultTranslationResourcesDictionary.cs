namespace Orbyss.Blazor.JsonForms.Core.Models;

/// <summary>
/// Default translation sections grouped by language code.
/// </summary>
public sealed class DefaultTranslationResourcesDictionary
    : Dictionary<string, IDictionary<string, TranslationSection>>
{
    /// <summary>Creates an empty dictionary with case-insensitive language keys.</summary>
    public DefaultTranslationResourcesDictionary()
        : base(StringComparer.OrdinalIgnoreCase)
    {
    }

    /// <summary>Creates a dictionary from an existing language map.</summary>
    public DefaultTranslationResourcesDictionary(
        IDictionary<string, IDictionary<string, TranslationSection>> dictionary)
        : base(dictionary, StringComparer.OrdinalIgnoreCase)
    {
    }
}
