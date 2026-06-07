using Microsoft.JSInterop.Implementation;
using Newtonsoft.Json.Linq;
using Orbyss.Blazor.JsonForms.Core.Context.Models;
using Orbyss.Blazor.JsonForms.Context.Notifications;
using Orbyss.Blazor.JsonForms.Core.Context.Notifications;
using Orbyss.Blazor.JsonForms.Context.Utils;
using Orbyss.Blazor.JsonForms.Core.Models;
using Orbyss.Blazor.JsonForms.Core;

namespace Orbyss.Blazor.JsonForms.Tests.Context;

public sealed class JsonFormContextTests
{
    private const string jsonSchema = "{\"properties\":{\"firstName\":{\"type\":\"string\", \"maxLength\": 6},\"surname\":{\"type\":\"string\"}}}";
    private const string translationSchema = "{\"resources\":{\"en\":{\"translation\":{\"firstName\":{\"label\":\"First Name\"},\"helperKey\":{\"label\":\"This is helpful info\"}}},\"nl\":{\"translation\":{\"firstName\":{\"label\":\"Voornaam\"}}}}}";
    private const string uiSchema = "{\"type\":\"VerticalLayout\",\"elements\":[{\"type\":\"Control\",\"scope\":\"#/properties/firstName\",\"options\":{\"readonly\":true,\"disabled\":true}},{\"type\":\"Control\",\"scope\":\"#/properties/surname\",\"options\":{\"hidden\":true},\"rule\":{\"effect\":\"Show\",\"condition\":{\"scope\":\"#/properties/firstName\",\"schema\":{\"minLength\":2}}}}],\"options\":{\"customOption\":\"custom-option-value\"}}";
    private const string uiSchemaWithCssClass = "{\"type\":\"VerticalLayout\",\"elements\":[{\"type\":\"Control\",\"scope\":\"#/properties/firstName\",\"options\":{\"cssClass\":\"my-class\"}}]}";
    private const string uiSchemaWithHelperIconTextLabel = "{\"type\":\"VerticalLayout\",\"elements\":[{\"type\":\"Control\",\"scope\":\"#/properties/firstName\",\"options\":{\"helperIconTextLabel\":\"helperKey\"}},{\"type\":\"Control\",\"scope\":\"#/properties/surname\",\"options\":{\"helperIconTextLabel\":\"Literal helper text\"}}]}";
    private const string uiSchemaWithDuplicateScope = "{\"type\":\"VerticalLayout\",\"elements\":[{\"type\":\"Control\",\"scope\":\"#/properties/firstName\"},{\"type\":\"Control\",\"scope\":\"#/properties/firstName\",\"options\":{\"hidden\":true}}]}";
    private const string uiSchemaWithHelperText = "{\"type\":\"VerticalLayout\",\"elements\":[{\"type\":\"Control\",\"scope\":\"#/properties/firstName\",\"options\":{\"helperTextLabel\":\"helperKey\"}},{\"type\":\"Control\",\"scope\":\"#/properties/surname\",\"options\":{\"helperTextLabel\":\"Literal helper text\"}}]}";
    private const string uiSchemaWithPrefixSuffix = "{\"type\":\"VerticalLayout\",\"elements\":[{\"type\":\"Control\",\"scope\":\"#/properties/age\",\"options\":{\"prefixLabel\":\"Age: \",\"suffixLabel\":\" years\"}}]}";
    private const string jsonSchemaWithMinMax = "{\"properties\":{\"age\":{\"type\":\"integer\",\"minimum\":0,\"maximum\":120}}}";
    private const string uiSchemaSimpleAge = "{\"type\":\"VerticalLayout\",\"elements\":[{\"type\":\"Control\",\"scope\":\"#/properties/age\"}]}";
    private const string uiSchemaWithEnumItemOptions = "{\"type\":\"VerticalLayout\",\"elements\":[{\"type\":\"Control\",\"scope\":\"#/properties/role\",\"options\":{\"enumItemOptions\":{\"admin\":{\"helperText\":\"Full access\"},\"user\":{\"helperText\":\"Standard access\"}}}}]}";
    private const string jsonSchemaWithEnum = "{\"properties\":{\"role\":{\"type\":\"string\",\"enum\":[\"admin\",\"user\",\"guest\"]}}}";

    [Xunit.Fact]
    public void When_Instantiate_Then_SetsUpContext()
    {
        // Arrange
        var formData = new JObject
        {
            ["firstName"] = "H"
        };

        var initOptions = new JsonFormOptions(
            jsonSchema,
            uiSchema,
            translationSchema
        )
        {
            Language = "nl",
            Disabled = true,
            ReadOnly = true,
            Data = formData
        };

        // Act
        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);

        // Assert
        Assert.That(sut.Disabled, Is.True);
        Assert.That(sut.ReadOnly, Is.True);

        var page = sut.GetPage(0);
        var verticalLayout = (FormVerticalLayoutContext)page.ElementContexts[0];
        var surnameElement = verticalLayout.Rows.First(x => x.Interpretation.Label?.Label == "surname");
        Assert.That(surnameElement.Hidden, Is.True);

        var formOption = sut.GetFormOption("customOption");
        Assert.That($"{formOption}", Is.EqualTo("custom-option-value"));
    }

    [Xunit.Fact]
    public void When_GetTranslatedLabel_And_DefaultTranslationExists_Then_ReturnsDefaultLabel()
    {
        // Arrange
        var initOptions = new JsonFormOptions(
            jsonSchema,
            uiSchema,
            translationSchema
        )
        {
            Language = "en",
            DefaultTranslations =
            {
                ["en"] = new Dictionary<string, TranslationSection>(StringComparer.OrdinalIgnoreCase)
                {
                    ["orbyss.form.button.submit"] = Label("Submit")
                }
            }
        };

        // Act
        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);

        // Assert
        Assert.That(sut.GetTranslatedLabel("orbyss.form.button.submit"), Is.EqualTo("Submit"));
    }

    [Xunit.Fact]
    public void When_GetTranslatedLabel_And_UserTranslationExists_Then_UserTranslationOverridesDefault()
    {
        // Arrange
        const string translationSchemaWithSubmit = """
            {
                "resources": {
                    "en": {
                        "translation": {
                            "orbyss.form.button.submit": { "label": "Send it" }
                        }
                    }
                }
            }
            """;

        var initOptions = new JsonFormOptions(
            jsonSchema,
            uiSchema,
            translationSchemaWithSubmit
        )
        {
            Language = "en",
            DefaultTranslations =
            {
                ["en"] = new Dictionary<string, TranslationSection>(StringComparer.OrdinalIgnoreCase)
                {
                    ["orbyss.form.button.submit"] = Label("Submit")
                }
            }
        };

        // Act
        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);

        // Assert
        Assert.That(sut.GetTranslatedLabel("orbyss.form.button.submit"), Is.EqualTo("Send it"));
    }

    [Xunit.Fact]
    public void When_ChangeDisabled_Then_PublishesEvent()
    {
        // Arrange
        int assertionValue = 0;
        var initOptions = new JsonFormOptions(
            jsonSchema,
            uiSchema,
            translationSchema
        );
        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);
        var subscriptionToken = sut.FormNotification.Subscribe(
            JsonFormNotificationType.OnDisabledChanged,
            () => assertionValue = 12
        );

        // Act
        sut.ChangeDisabled(true);

        // Assert
        Assert.That(assertionValue, Is.EqualTo(12));
    }

    private static TranslationSection Label(string label)
        => new(label, Error: null, Enums: null, NestedSections: null);

    [Xunit.Fact]
    public void When_ChangeLanguage_Then_PublishesEvent()
    {
        // Arrange
        int assertionValue = 0;
        var initOptions = new JsonFormOptions(
            jsonSchema,
            uiSchema,
            translationSchema
        );
        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);
        var subscriptionToken = sut.FormNotification.Subscribe(
            JsonFormNotificationType.OnLanguageChanged,
            () => assertionValue = 12
        );

        // Act
        sut.ChangeLanguage("en");

        // Assert
        Assert.That(assertionValue, Is.EqualTo(12));
    }

    [Xunit.Fact]
    public void When_ChangeReadOnly_Then_PublishesEvent()
    {
        // Arrange
        int assertionValue = 0;
        var initOptions = new JsonFormOptions(
            jsonSchema,
            uiSchema,
            translationSchema
        );
        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);
        var subscriptionToken = sut.FormNotification.Subscribe(
            JsonFormNotificationType.OnReadOnlyChanged,
            () => assertionValue = 12
        );

        // Act
        sut.ChangeReadOnly(true);

        // Assert
        Assert.That(assertionValue, Is.EqualTo(12));
    }

    [Xunit.Fact]
    public void When_Validate_Then_PublishesEvent()
    {
        // Arrange
        int assertionValue = 0;
        var initOptions = new JsonFormOptions(
            jsonSchema,
            uiSchema,
            translationSchema
        );
        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);
        var subscriptionToken = sut.FormNotification.Subscribe(
            JsonFormNotificationType.OnDataValidated,
            () => assertionValue = 12
        );

        // Act
        _ = sut.Validate();

        // Assert
        Assert.That(assertionValue, Is.EqualTo(12));
    }

    [Xunit.Fact]
    public void When_GetValue_Then_ReturnsControlJsonToken()
    {
        // Arrange
        var formData = new JObject
        {
            ["firstName"] = "Johannes"
        };

        var initOptions = new JsonFormOptions(jsonSchema, uiSchema, translationSchema)
        {
            Data = formData
        };
        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);
        var page = sut.GetPage(0);
        var verticalLayout = (FormVerticalLayoutContext)page.ElementContexts[0];
        var firstNameContext = verticalLayout.Rows.First(x => x.Interpretation.Label?.Label == "firstName");

        // Act
        var result = sut.GetValue(firstNameContext.Id);

        // Assert
        Assert.That($"{result}", Is.EqualTo("Johannes"));
    }

    [Xunit.Fact]
    public void When_GetValue_And_ContextIsNotFound_Then_ThrowsException()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var initOptions = new JsonFormOptions(jsonSchema, uiSchema, translationSchema);
        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);

        // Act & Assert
        var e = Assert.Throws<InvalidOperationException>(() =>
        {
            _ = sut.GetValue(invalidId);
        });
        Assert.That(e.Message, Is.EqualTo($"Could not find context by id '{invalidId}'"));
    }

    [Xunit.Fact]
    public void When_GetValue_And_ContextIsNotControl_Then_ThrowsException()
    {
        // Arrange
        var initOptions = new JsonFormOptions(jsonSchema, uiSchema, translationSchema);
        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);

        var page = sut.GetPage(0);
        var verticalLayout = (FormVerticalLayoutContext)page.ElementContexts[0];

        // Act & Assert
        var e = Assert.Throws<InvalidCastException>(() =>
        {
            _ = sut.GetValue(verticalLayout.Id);
        });
        Assert.That(e.Message, Is.EqualTo($"Context of type '{verticalLayout.GetType()}' could not be cast to type '{typeof(FormControlContext)}'"));
    }

    [Xunit.Fact]
    public void When_UpdateValue_Then_UpdatesContextToken_And_EnforcesRules()
    {
        // Arrange
        var formData = new JObject
        {
            ["firstName"] = "H"
        };

        var initOptions = new JsonFormOptions(jsonSchema, uiSchema, translationSchema)
        {
            Data = formData
        };
        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);
        var page = sut.GetPage(0);
        var verticalLayout = (FormVerticalLayoutContext)page.ElementContexts[0];
        var firstNameContext = verticalLayout.Rows.First(x => x.Interpretation.Label?.Label == "firstName");
        var surnameElement = verticalLayout.Rows.First(x => x.Interpretation.Label?.Label == "surname");

        // Pre-Assert
        Assert.That(surnameElement.Hidden, Is.True);

        // Act
        sut.UpdateValue(firstNameContext.Id, JValue.CreateString("Johannes"));

        // Assert
        var updatedFirstNameToken = sut.GetValue(firstNameContext.Id);

        Assert.That($"{updatedFirstNameToken}", Is.EqualTo("Johannes"));
        Assert.That(surnameElement.Hidden, Is.False);
    }

    [Xunit.Fact]
    public void When_UpdatFormData_Then_UpdatesContextToken_And_EnforcesRules_And_Validates()
    {
        // Arrange
        var formData = new JObject
        {
            ["firstName"] = "H"
        };

        var initOptions = new JsonFormOptions(jsonSchema, uiSchema, translationSchema)
        {
            Data = formData
        };
        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);
        var page = sut.GetPage(0);
        var verticalLayout = (FormVerticalLayoutContext)page.ElementContexts[0];
        var firstNameContext = verticalLayout.Rows.First(x => x.Interpretation.Label?.Label == "firstName");
        var surnameElement = verticalLayout.Rows.First(x => x.Interpretation.Label?.Label == "surname");

        sut.Validate();

        // Pre-Assert
        Assert.That(surnameElement.Hidden, Is.True);
        var firstNameErrorText = sut.GetDataContextError(firstNameContext.Id);
        Assert.That(firstNameErrorText, Is.Null.Or.Empty);

        // Act
        sut.UpdateFormData((formData) => formData["firstName"] = "Johannes");

        // Assert
        var updatedFirstNameToken = sut.GetValue(firstNameContext.Id);

        Assert.That($"{updatedFirstNameToken}", Is.EqualTo("Johannes"));
        Assert.That(surnameElement.Hidden, Is.False);
        firstNameErrorText = sut.GetDataContextError(firstNameContext.Id);
        Assert.That(firstNameErrorText, Is.EqualTo(DefaultJsonFormValidationMessages.MaxLength));
    }

    [Xunit.Fact]
    public void When_GetFOrmData_AndCOntrolsHidden_Then_RemovesHiddenValues()
    {
        // Arrange
        const string surname = "Hellooo";
        var formData = new JObject
        {
            ["firstName"] = "H",
            ["surname"] = surname
        };        

        var initOptions = new JsonFormOptions(jsonSchema, uiSchema, translationSchema)
        {
            Data = formData
        };
        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);
        var page = sut.GetPage(0);
        var verticalLayout = (FormVerticalLayoutContext)page.ElementContexts[0];
        var surnameElement = verticalLayout.Rows.First(x => x.Interpretation.Label?.Label == "surname");

        // Pre-Assert
        Assert.That(surnameElement.Hidden, Is.True);

        // Act
        var result = sut.GetFormData();

        // Assert        
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<JObject>());

        var jobject = (JObject)result;
        Assert.That(jobject.ContainsKey("surname"), Is.True);
        Assert.That($"{jobject["surname"]}", Is.Not.EqualTo(surname));
    }

    [Xunit.Fact]
    public void When_GetFormData_And_HiddenContextSharesPathWithDisplayedContext_Then_ValueIsPreserved()
    {
        // Arrange — two controls bound to the same path, one hidden, one displayed
        var formData = new JObject
        {
            ["firstName"] = "Johannes"
        };

        var initOptions = new JsonFormOptions(jsonSchema, uiSchemaWithDuplicateScope, translationSchema)
        {
            Data = formData
        };
        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);

        // Act
        var result = sut.GetFormData();

        // Assert — the displayed context keeps the value alive; hidden sibling must not null it out
        var jobject = (JObject)result;
        Assert.That($"{jobject["firstName"]}", Is.EqualTo("Johannes"));
    }

    [Xunit.Fact]
    public void When_GetCssClass_And_OptionIsSet_Then_ReturnsOptionValue()
    {
        // Arrange
        var initOptions = new JsonFormOptions(jsonSchema, uiSchemaWithCssClass, translationSchema);
        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);
        var page = sut.GetPage(0);
        var verticalLayout = (FormVerticalLayoutContext)page.ElementContexts[0];
        var firstNameContext = verticalLayout.Rows.First(x => x.Interpretation.Label?.Label == "firstName");

        // Act
        var result = sut.GetCssClass(firstNameContext.Id);

        // Assert
        Assert.That(result, Is.EqualTo("my-class"));
    }

    [Xunit.Fact]
    public void When_GetCssClass_And_OptionIsNotSet_Then_ReturnsNull()
    {
        // Arrange
        var initOptions = new JsonFormOptions(jsonSchema, uiSchema, translationSchema);
        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);
        var page = sut.GetPage(0);
        var verticalLayout = (FormVerticalLayoutContext)page.ElementContexts[0];
        var firstNameContext = verticalLayout.Rows.First(x => x.Interpretation.Label?.Label == "firstName");

        // Act
        var result = sut.GetCssClass(firstNameContext.Id);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Xunit.Fact]
    public void When_GetHelperIconText_And_OptionValueIsI18nKey_Then_ReturnsTranslatedLabel()
    {
        // Arrange — "helperKey" resolves to "This is helpful info" via the en translation
        var initOptions = new JsonFormOptions(jsonSchema, uiSchemaWithHelperIconTextLabel, translationSchema)
        {
            Language = "en"
        };
        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);
        var page = sut.GetPage(0);
        var verticalLayout = (FormVerticalLayoutContext)page.ElementContexts[0];
        var firstNameContext = verticalLayout.Rows.First(x => x.Interpretation.Label?.Label == "firstName");

        // Act
        var result = sut.GetHelperIconText(firstNameContext.Id);

        // Assert
        Assert.That(result, Is.EqualTo("This is helpful info"));
    }

    [Xunit.Fact]
    public void When_GetHelperIconText_And_OptionValueIsLiteralString_Then_ReturnsLiteralValue()
    {
        // Arrange — "Literal helper text" has no matching i18n key, falls back to the literal value
        var initOptions = new JsonFormOptions(jsonSchema, uiSchemaWithHelperIconTextLabel, translationSchema)
        {
            Language = "en"
        };
        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);
        var page = sut.GetPage(0);
        var verticalLayout = (FormVerticalLayoutContext)page.ElementContexts[0];
        var surnameContext = verticalLayout.Rows.First(x => x.Interpretation.Label?.Label == "surname");

        // Act
        var result = sut.GetHelperIconText(surnameContext.Id);

        // Assert
        Assert.That(result, Is.EqualTo("Literal helper text"));
    }

    [Xunit.Fact]
    public void When_GetHelperIconText_And_OptionIsNotSet_Then_ReturnsNull()
    {
        // Arrange
        var initOptions = new JsonFormOptions(jsonSchema, uiSchema, translationSchema);
        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);
        var page = sut.GetPage(0);
        var verticalLayout = (FormVerticalLayoutContext)page.ElementContexts[0];
        var firstNameContext = verticalLayout.Rows.First(x => x.Interpretation.Label?.Label == "firstName");

        // Act
        var result = sut.GetHelperIconText(firstNameContext.Id);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Xunit.Fact]
    public void When_GetHelperText_And_OptionValueIsI18nKey_Then_ReturnsTranslatedLabel()
    {
        var initOptions = new JsonFormOptions(jsonSchema, uiSchemaWithHelperText, translationSchema) { Language = "en" };
        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);
        var page = sut.GetPage(0);
        var verticalLayout = (FormVerticalLayoutContext)page.ElementContexts[0];
        var firstNameContext = verticalLayout.Rows.First(x => x.Interpretation.Label?.Label == "firstName");

        var result = sut.GetHelperText(firstNameContext.Id);

        Assert.That(result, Is.EqualTo("This is helpful info"));
    }

    [Xunit.Fact]
    public void When_GetHelperText_And_OptionValueIsLiteralString_Then_ReturnsLiteralValue()
    {
        var initOptions = new JsonFormOptions(jsonSchema, uiSchemaWithHelperText, translationSchema) { Language = "en" };
        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);
        var page = sut.GetPage(0);
        var verticalLayout = (FormVerticalLayoutContext)page.ElementContexts[0];
        var surnameContext = verticalLayout.Rows.First(x => x.Interpretation.Label?.Label == "surname");

        var result = sut.GetHelperText(surnameContext.Id);

        Assert.That(result, Is.EqualTo("Literal helper text"));
    }

    [Xunit.Fact]
    public void When_GetHelperText_And_OptionIsNotSet_Then_ReturnsNull()
    {
        var initOptions = new JsonFormOptions(jsonSchema, uiSchema, translationSchema);
        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);
        var page = sut.GetPage(0);
        var verticalLayout = (FormVerticalLayoutContext)page.ElementContexts[0];
        var firstNameContext = verticalLayout.Rows.First(x => x.Interpretation.Label?.Label == "firstName");

        var result = sut.GetHelperText(firstNameContext.Id);

        Assert.That(result, Is.Null);
    }

    [Xunit.Fact]
    public void When_GetPrefixText_And_OptionIsSet_Then_ReturnsValue()
    {
        var initOptions = new JsonFormOptions(jsonSchemaWithMinMax, uiSchemaWithPrefixSuffix, translationSchema);
        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);
        var page = sut.GetPage(0);
        var verticalLayout = (FormVerticalLayoutContext)page.ElementContexts[0];
        var ageContext = verticalLayout.Rows.First(x => x.Interpretation.Label?.Label == "age");

        var result = sut.GetPrefixText(ageContext.Id);

        Assert.That(result, Is.EqualTo("Age: "));
    }

    [Xunit.Fact]
    public void When_GetSuffixText_And_OptionIsSet_Then_ReturnsValue()
    {
        var initOptions = new JsonFormOptions(jsonSchemaWithMinMax, uiSchemaWithPrefixSuffix, translationSchema);
        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);
        var page = sut.GetPage(0);
        var verticalLayout = (FormVerticalLayoutContext)page.ElementContexts[0];
        var ageContext = verticalLayout.Rows.First(x => x.Interpretation.Label?.Label == "age");

        var result = sut.GetSuffixText(ageContext.Id);

        Assert.That(result, Is.EqualTo(" years"));
    }

    [Xunit.Fact]
    public void When_InterpretControl_And_SchemaHasMinMax_Then_InterpretationContainsMinMax()
    {
        var initOptions = new JsonFormOptions(jsonSchemaWithMinMax, uiSchemaSimpleAge, translationSchema);
        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);
        var page = sut.GetPage(0);
        var verticalLayout = (FormVerticalLayoutContext)page.ElementContexts[0];
        var ageContext = (FormControlContext)verticalLayout.Rows.First(x => x.Interpretation.Label?.Label == "age");

        Assert.That(ageContext.Interpretation.Minimum, Is.EqualTo(0));
        Assert.That(ageContext.Interpretation.Maximum, Is.EqualTo(120));
    }

    [Xunit.Fact]
    public void When_GetTranslatedEnumItems_And_EnumItemOptionsSet_Then_ItemsHaveHelperText()
    {
        var initOptions = new JsonFormOptions(jsonSchemaWithEnum, uiSchemaWithEnumItemOptions, translationSchema);
        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);
        var page = sut.GetPage(0);
        var verticalLayout = (FormVerticalLayoutContext)page.ElementContexts[0];
        var roleContext = verticalLayout.Rows.First(x => x.Interpretation.Label?.Label == "role");

        var items = sut.GetTranslatedEnumItems(roleContext.Id).ToList();

        Assert.That(items.First(x => x.Value == "admin").HelperText, Is.EqualTo("Full access"));
        Assert.That(items.First(x => x.Value == "user").HelperText, Is.EqualTo("Standard access"));
        Assert.That(items.First(x => x.Value == "guest").HelperText, Is.Null.Or.Empty);
    }

    // ── FindControl / FindControls ──────────────────────────────────────────

    [Xunit.Fact]
    public void When_FindControl_ByDataPath_Then_ReturnsCorrectContext()
    {
        var initOptions = new JsonFormOptions(jsonSchema, uiSchema, translationSchema);
        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);

        var result = sut.FindControl(c => c.AbsoluteDataJsonPath == "$.firstName");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.AbsoluteDataJsonPath, Is.EqualTo("$.firstName"));
    }

    [Xunit.Fact]
    public void When_FindControl_NoMatch_Then_ReturnsNull()
    {
        var initOptions = new JsonFormOptions(jsonSchema, uiSchema, translationSchema);
        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);

        var result = sut.FindControl(c => c.AbsoluteDataJsonPath == "nonExistentField");

        Assert.That(result, Is.Null);
    }

    [Xunit.Fact]
    public void When_FindControls_Then_ReturnsAllMatching()
    {
        var initOptions = new JsonFormOptions(jsonSchema, uiSchema, translationSchema);
        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);

        var results = sut.FindControls(_ => true).ToList();

        Assert.That(results.Count, Is.EqualTo(2));
        Assert.That(results.Any(c => c.AbsoluteDataJsonPath == "$.firstName"), Is.True);
        Assert.That(results.Any(c => c.AbsoluteDataJsonPath == "$.surname"), Is.True);
    }

    // ── OnControlValueChanged ───────────────────────────────────────────────

    [Xunit.Fact]
    public async Task When_NotifyControlValueChanged_And_HandlerRegistered_Then_HandlerIsCalled()
    {
        var initOptions = new JsonFormOptions(jsonSchema, uiSchema, translationSchema);

        FormControlContext? capturedControl = null;
        initOptions.OnControlValueChanged += (control, _) =>
        {
            capturedControl = control;
            return Task.CompletedTask;
        };

        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);
        var page = sut.GetPage(0);
        var verticalLayout = (FormVerticalLayoutContext)page.ElementContexts[0];
        var firstNameContext = verticalLayout.Rows.First(x => x.Interpretation.Label?.Label == "firstName");

        await sut.NotifyControlValueChanged(firstNameContext.Id);

        Assert.That(capturedControl, Is.Not.Null);
        Assert.That(capturedControl!.Id, Is.EqualTo(firstNameContext.Id));
    }

    [Xunit.Fact]
    public async Task When_NotifyControlValueChanged_And_MultipleHandlers_Then_AllAreCalled()
    {
        var initOptions = new JsonFormOptions(jsonSchema, uiSchema, translationSchema);

        var callCount = 0;
        initOptions.OnControlValueChanged += (_, _) => { callCount++; return Task.CompletedTask; };
        initOptions.OnControlValueChanged += (_, _) => { callCount++; return Task.CompletedTask; };

        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);
        var page = sut.GetPage(0);
        var verticalLayout = (FormVerticalLayoutContext)page.ElementContexts[0];
        var firstNameContext = verticalLayout.Rows.First(x => x.Interpretation.Label?.Label == "firstName");

        await sut.NotifyControlValueChanged(firstNameContext.Id);

        Assert.That(callCount, Is.EqualTo(2));
    }

    [Xunit.Fact]
    public async Task When_NotifyControlValueChanged_And_NoHandlerRegistered_Then_DoesNotThrow()
    {
        var initOptions = new JsonFormOptions(jsonSchema, uiSchema, translationSchema);
        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);
        var page = sut.GetPage(0);
        var verticalLayout = (FormVerticalLayoutContext)page.ElementContexts[0];
        var firstNameContext = verticalLayout.Rows.First(x => x.Interpretation.Label?.Label == "firstName");

        Assert.DoesNotThrowAsync(() => sut.NotifyControlValueChanged(firstNameContext.Id));
    }

    [Xunit.Fact]
    public async Task When_NotifyControlInputChanged_And_HandlerRegistered_Then_HandlerIsCalled()
    {
        var initOptions = new JsonFormOptions(jsonSchema, uiSchema, translationSchema);

        FormControlContext? capturedControl = null;
        initOptions.OnControlInputChanged += (control, _) =>
        {
            capturedControl = control;
            return Task.CompletedTask;
        };

        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);
        var page = sut.GetPage(0);
        var verticalLayout = (FormVerticalLayoutContext)page.ElementContexts[0];
        var firstNameContext = verticalLayout.Rows.First(x => x.Interpretation.Label?.Label == "firstName");

        await sut.NotifyControlInputChanged(firstNameContext.Id);

        Assert.That(capturedControl, Is.Not.Null);
        Assert.That(capturedControl!.Id, Is.EqualTo(firstNameContext.Id));
    }

    [Xunit.Fact]
    public async Task When_HandlerCallsUpdateValue_Via_FindControl_Then_ValueIsUpdated()
    {
        var initOptions = new JsonFormOptions(jsonSchema, uiSchema, translationSchema);

        initOptions.OnControlValueChanged += (control, form) =>
        {
            // Simulate: when firstName changes, write a derived value to surname
            if (control.AbsoluteDataJsonPath == "$.firstName")
            {
                var surnameCtx = form.FindControl(c => c.AbsoluteDataJsonPath == "$.surname");
                if (surnameCtx is not null)
                    form.UpdateValue(surnameCtx.Id, JToken.FromObject("auto-filled"));
            }
            return Task.CompletedTask;
        };

        var sut = JsonFormContextBuilder.BuildAndInstantiate(initOptions);
        var page = sut.GetPage(0);
        var verticalLayout = (FormVerticalLayoutContext)page.ElementContexts[0];
        var firstNameContext = verticalLayout.Rows.First(x => x.Interpretation.Label?.Label == "firstName");
        var surnameContext = verticalLayout.Rows.First(x => x.Interpretation.Label?.Label == "surname");

        sut.UpdateValue(firstNameContext.Id, JToken.FromObject("John"));
        await sut.NotifyControlValueChanged(firstNameContext.Id);

        var surnameValue = sut.GetValue(surnameContext.Id);
        Assert.That($"{surnameValue}", Is.EqualTo("auto-filled"));
    }
}

