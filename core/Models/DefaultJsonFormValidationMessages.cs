namespace Orbyss.Blazor.JsonForms.Core.Models;

public static class DefaultJsonFormValidationMessages
{
    public static string Default { get; set; } = "Invalid";
    public static string Minimum { get; set; } = "Too low";
    public static string Maximum { get; set; } = "Too high";
    public static string MaxItems { get; set; } = "Too many items";
    public static string MinItems { get; set; } = "Too few items";
    public static string MaxLength { get; set; } = "Too long";
    public static string MinLength { get; set; } = "Too short";
    public static string Contains { get; set; } = "Missing required value";
    public static string Required { get; set; } = "Required";
    public static string Pattern { get; set; } = "Invalid format";
    public static string Const { get; set; } = "Invalid value";
}