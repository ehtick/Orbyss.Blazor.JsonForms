namespace Orbyss.Blazor.JsonForms.Utils;

/// <summary>
/// Utility for merging CSS class strings from component defaults and UI schema options.
/// </summary>
public static class CssClassHelper
{
    /// <summary>
    /// Merges a component's default CSS class with a value from the UI schema <c>cssClass</c> option.
    /// <list type="bullet">
    ///   <item>If <paramref name="optionClass"/> is null or whitespace, returns <paramref name="defaultClass"/> unchanged.</item>
    ///   <item>
    ///     If <paramref name="optionClass"/> starts with <c>!</c>, the remainder <em>replaces</em>
    ///     the default entirely — use this when you want to fully own the class string
    ///     rather than augment it.
    ///   </item>
    ///   <item>Otherwise, <paramref name="optionClass"/> is appended to <paramref name="defaultClass"/>.</item>
    /// </list>
    /// </summary>
    public static string? Merge(string? defaultClass, string? optionClass)
    {
        if (string.IsNullOrWhiteSpace(optionClass))
            return defaultClass;

        if (optionClass.StartsWith('!'))
        {
            var replaced = optionClass[1..].Trim();
            return string.IsNullOrWhiteSpace(replaced) ? null : replaced;
        }

        return string.Join(" ", new[] { defaultClass, optionClass }
            .Where(c => !string.IsNullOrWhiteSpace(c)));
    }
}
