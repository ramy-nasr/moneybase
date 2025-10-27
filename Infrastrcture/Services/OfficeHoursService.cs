using Infrastructure.Abstraction;
using Infrastructure.Config;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public sealed class OfficeHoursService(IOptions<SupportConfig> config) : IOfficeHoursService
{
    private readonly IOptions<SupportConfig> _config = config;
    private readonly TimeZoneInfo _tz = TimeZoneInfo.FindSystemTimeZoneById(config.Value.LocalTimeZoneId);

    public bool IsOfficeHours(DateTime utcNow)
    {
        var local = TimeZoneInfo.ConvertTime(utcNow, _tz);
        return _config.Value.Office.IsOfficeHours(local);
    }
}
