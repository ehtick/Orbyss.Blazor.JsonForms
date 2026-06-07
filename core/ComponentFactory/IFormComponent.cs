using Newtonsoft.Json.Linq;

namespace Orbyss.Blazor.JsonForms.Core.ComponentFactory;

/// <summary>
/// Contract that every data-bound form input component must implement.
///
/// <para>
/// The form engine validates that input component types assigned to factory slots implement
/// this interface. Attempting to register a type that does not implement <see cref="IFormComponent"/>
/// throws an <see cref="InvalidOperationException"/> at registration time — before any rendering
/// occurs, so mistakes surface immediately.
/// </para>
///
/// <para>
/// The interface declares a single conversion method — JToken (the form's internal storage format)
/// to the component's native CLR value type. The engine calls this once per component instance
/// to populate the initial <c>Value</c> parameter.
/// </para>
///
/// <para>
/// For the standard implementation inherit from
/// <c>FormInputComponentBase&lt;TValue&gt;</c> — it wires up <c>Value</c>,
/// <c>ValueChanged</c>, <c>OnValueChanged</c>, <c>Disabled</c>, <c>ReadOnly</c>, <c>Label</c>
/// and all other standard parameters, and provides a default <see cref="ConvertFromJToken"/>
/// using <c>JToken.ToObject&lt;TValue&gt;</c>. Override the method for types that need custom
/// conversion (e.g. <c>DateTimeUtcTicks</c>, <c>DateOnly</c>).
/// </para>
/// </summary>
public interface IFormComponent
{
    /// <summary>
    /// Converts a raw JSON token from the form data store into the component's native value type.
    /// Return <c>null</c> when the token is null or represents a JSON null.
    /// </summary>
    object? ConvertFromJToken(JToken? token);
}
