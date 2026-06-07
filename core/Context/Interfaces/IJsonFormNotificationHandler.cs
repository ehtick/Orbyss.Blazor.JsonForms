using Orbyss.Blazor.JsonForms.Core.Context.Notifications;

namespace Orbyss.Blazor.JsonForms.Core.Context.Interfaces;

public interface IJsonFormNotificationHandler : IJsonFormNotification
{
    void Notify(JsonFormNotificationType type);
}

public interface IJsonFormNotification
{
    IDisposable Subscribe(JsonFormNotificationType type, Action callback);
}
