namespace Orbyss.Blazor.JsonForms.Core.Models;

public readonly struct DateTimeUtcTicks(long utcTicks)
{
    public readonly DateTimeOffset DateTime = new(utcTicks, TimeSpan.Zero);
    public readonly long UtcTicks = utcTicks;
}
