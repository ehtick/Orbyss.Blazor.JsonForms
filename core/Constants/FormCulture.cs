using System.Globalization;

namespace Orbyss.Blazor.JsonForms.Core.Constants;

/// <summary>
/// Default culture used as the initial value for the <c>Culture</c> parameter on all form
/// components. Set <see cref="Instance"/> once at application startup to change the global default.
/// </summary>
public static class FormCulture
{
    public static CultureInfo Instance { get; set; } = new CultureInfo("en-US");
}
