using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Linq;
using Orbyss.Blazor.JsonForms.Core.ComponentFactory;

namespace Orbyss.Blazor.JsonForms.Tests.ComponentFactory;

public class ComponentFactoryBaseTests
{
    [Xunit.Fact]
    public void When_ApplyUiSchemaParameters_And_ParameterIsEnum_Then_ConvertsStringToEnum()
    {
        // Arrange
        var instance = new FormComponentInstance(typeof(TestComponent));
        var parameters = JObject.Parse("""{ "Mode": "Advanced" }""");

        // Act
        TestFactory.ApplyParameters(instance, parameters);

        // Assert
        Assert.That(instance.Parameters["Mode"], Is.EqualTo(TestMode.Advanced));
    }

    [Xunit.Fact]
    public void When_ApplyUiSchemaParameters_And_ParameterIsNullableEnum_Then_ConvertsStringToEnum()
    {
        // Arrange
        var instance = new FormComponentInstance(typeof(TestComponent));
        var parameters = JObject.Parse("""{ "OptionalMode": "Basic" }""");

        // Act
        TestFactory.ApplyParameters(instance, parameters);

        // Assert
        Assert.That(instance.Parameters["OptionalMode"], Is.EqualTo(TestMode.Basic));
    }

    [Xunit.Fact]
    public void When_ApplyUiSchemaParameters_And_ParameterIsScalar_Then_ConvertsValue()
    {
        // Arrange
        var instance = new FormComponentInstance(typeof(TestComponent));
        var parameters = JObject.Parse("""{ "Maximum": 42 }""");

        // Act
        TestFactory.ApplyParameters(instance, parameters);

        // Assert
        Assert.That(instance.Parameters["Maximum"], Is.EqualTo(42));
    }

    private sealed class TestFactory : ComponentFactoryBase
    {
        public static void ApplyParameters(IComponentInstance instance, JToken parameters)
        {
            ApplyUiSchemaParameters(instance, parameters);
        }
    }

    private sealed class TestComponent : ComponentBase
    {
        [Parameter]
        public TestMode Mode { get; set; }

        [Parameter]
        public TestMode? OptionalMode { get; set; }

        [Parameter]
        public int Maximum { get; set; }
    }

    private enum TestMode
    {
        Basic,
        Advanced
    }
}

