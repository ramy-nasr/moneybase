using Domain.Abstractions;

namespace Infrastructure.Services;

public sealed class SystemClockService : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime ToLocal(DateTime utc, string timeZoneId)
    {
        var tz = timeZoneInfo(timeZoneId);
        return TimeZoneInfo.ConvertTimeFromUtc(utc, tz);
    }

    private static TimeZoneInfo timeZoneInfo(string id) =>
        TimeZoneInfo.FindSystemTimeZoneById(id);
}
