using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Orbyss.Blazor.JsonForms.ComponentFactory;
using Orbyss.Blazor.JsonForms.Context;
using Orbyss.Blazor.JsonForms.Context.Interfaces;
using Orbyss.Blazor.JsonForms.Context.Notifications;
using Orbyss.Blazor.JsonForms.Core.ComponentFactory;
using Orbyss.Blazor.JsonForms.Interpretation;
using Orbyss.Blazor.JsonForms.Interpretation.Interfaces;

namespace Orbyss.Blazor.JsonForms.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJsonForms(
        this IServiceCollection services,
        Func<IServiceProvider, IJsonFormContext>? jsonFormContextFactory = null,
        Action<FormComponentFactoryOptions>? configureFactories = null
    )
    {
        services
            .AddScoped<IControlTypeInterpreter, ControlTypeInterpreter>()
            .AddScoped<IJsonPathInterpreter, JsonPathInterpreter>()
            .AddScoped<IFormUiSchemaInterpreter, FormUiSchemaInterpreter>()
            .AddScoped<IJsonTransformer, JlioJsonTransformer>()
            .AddScoped<IFormRuleEnforcer, FormRuleEnforcer>()
            .AddScoped<IFormElementContextFactory, FormElementContextFactory>()

            .AddTransient<IJsonFormNotificationHandler, JsonFormNotificationHandler>()
            .AddTransient<IJsonFormDataContext, JsonFormDataContext>()
            .AddTransient<IJsonFormTranslationContext, JsonFormTranslationContext>();

        if (jsonFormContextFactory is not null)
        {
            services.AddTransient(jsonFormContextFactory);
        }
        else
        {
            services.AddTransient<IJsonFormContext, JsonFormContext>();
        }

        AddComponentFactories(services, configureFactories);

        return services;
    }

    /// <summary>
    /// Registers the composite <see cref="IFormComponentFactory"/> and each per-slot sub-factory as
    /// configured singletons. Sub-factories are registered with <c>TryAdd</c>, so a custom
    /// implementation registered before <c>AddJsonForms</c> takes precedence over the default —
    /// in which case the matching configure callback is ignored.
    /// </summary>
    private static void AddComponentFactories(
        IServiceCollection services,
        Action<FormComponentFactoryOptions>? configureFactories)
    {
        var options = new FormComponentFactoryOptions();
        configureFactories?.Invoke(options);

        services.TryAddSingleton<IControlComponentFactory>(_ =>
        {
            var factory = new ControlComponentFactory();
            options.ConfigureControls?.Invoke(factory);
            return factory;
        });

        services.TryAddSingleton<IButtonComponentFactory>(_ =>
        {
            var factory = new ButtonComponentFactory();
            options.ConfigureButtons?.Invoke(factory);
            return factory;
        });

        services.TryAddSingleton<INavigationComponentFactory>(_ =>
        {
            var factory = new NavigationComponentFactory();
            options.ConfigureNavigation?.Invoke(factory);
            return factory;
        });

        services.TryAddSingleton<IListComponentFactory>(_ =>
        {
            var factory = new ListComponentFactory();
            options.ConfigureList?.Invoke(factory);
            return factory;
        });

        services.TryAddSingleton<IActionButtonComponentFactory>(_ =>
        {
            var factory = new ActionButtonComponentFactory();
            options.ConfigureActionButtons?.Invoke(factory);
            return factory;
        });

        services.TryAddSingleton<IArrayLayoutComponentFactory>(_ =>
        {
            var factory = new ArrayLayoutComponentFactory();
            options.ConfigureArrayLayout?.Invoke(factory);
            return factory;
        });

        services.TryAddSingleton<IFormComponentFactory, FormComponentFactory>();
    }
}
