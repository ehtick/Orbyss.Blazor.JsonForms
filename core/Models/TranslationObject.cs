namespace Orbyss.Blazor.JsonForms.Core.Models;

public sealed class TranslationObject(string language, IDictionary<string, TranslationSection> sections)
{
    public string Language { get; } = language;

    public IDictionary<string, TranslationSection> Sections { get; } = sections;
}