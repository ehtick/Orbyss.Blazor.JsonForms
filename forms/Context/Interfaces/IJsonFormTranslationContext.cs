using Newtonsoft.Json.Schema;
using Orbyss.Blazor.JsonForms.Core.Interpretation;
using Orbyss.Blazor.JsonForms.Core.Models;

namespace Orbyss.Blazor.JsonForms.Context.Interfaces;

public interface IJsonFormTranslationContext
{
    void Instantiate(
        TranslationSchema translationSchema,
        JSchema dataSchema,
        DefaultTranslationResourcesDictionary? defaultTranslations = null);

    string TranslateErrors(string? language, IEnumerable<ErrorType> errors, UiSchemaControlInterpretationBase controlInterpretation);

    string? TranslateLabel(string? language, UiSchemaLabelInterpretation labelInterpretation);

    string? TranslateLabel(string? language, UiSchemaControlInterpretationBase controlInterpretation);

    IEnumerable<TranslatedEnumItem>? TranslateEnum(string? language, UiSchemaControlInterpretation controlInterpretation);

    IEnumerable<string> GetAvailableLanguages();
}
