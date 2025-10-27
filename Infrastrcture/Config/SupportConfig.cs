namespace Infrastructure.Config;

public sealed class SupportConfig
{
    public string LocalTimeZoneId { get; set; } = "Africa/Cairo";
    public int BaseConcurrencyPerAgent { get; set; } = 10;
    public decimal QueueFactor { get; set; } = 1.5m;
    public int InactivitySeconds { get; set; } = 3;
    public OfficeConfig Office { get; set; } = new();
    public TeamConfig Teams { get; set; } = new();
    public OverflowConfig Overflow { get; set; } = new();
}

public sealed class OfficeConfig
{
    public string Start { get; set; } = "09:00";
    public string End { get; set; } = "18:00";

    public bool IsOfficeHours(DateTime localNow)
    {
        var start = TimeOnly.Parse(Start);
        var end = TimeOnly.Parse(End);
        var t = TimeOnly.FromDateTime(localNow);
        return start <= end ? (t >= start && t <= end) : (t >= start || t <= end);
    }
}

public sealed class OverflowConfig
{
    public int JuniorCount { get; set; } = 6;
    public decimal JuniorEfficiency { get; set; } = 0.4m;
}

public sealed class TeamConfig
{
    public string ShiftAStart { get; set; } = "08:00";
    public string ShiftAEnd { get; set; } = "16:00";
    public string ShiftBStart { get; set; } = "16:00";
    public string ShiftBEnd { get; set; } = "00:00";
    public string ShiftCStart { get; set; } = "00:00";
    public string ShiftCEnd { get; set; } = "08:00";

    public int TeamA_TeamLead { get; set; } = 1;
    public int TeamA_Mid { get; set; } = 2;
    public int TeamA_Junior { get; set; } = 1;

    public int TeamB_Senior { get; set; } = 1;
    public int TeamB_Mid { get; set; } = 1;
    public int TeamB_Junior { get; set; } = 2;

    public int TeamC_Mid { get; set; } = 2;

    public static decimal EfficiencyFor(string seniority) => seniority switch
    {
        "Junior" => 0.4m,
        "Mid" => 0.6m,
        "Senior" => 0.8m,
        "TeamLead" => 0.5m,
        _ => 0.0m
    };

    public string GetActiveBaseTeam(TimeOnly t)
    {
        var a = (TimeOnly.Parse(ShiftAStart), TimeOnly.Parse(ShiftAEnd));
        var b = (TimeOnly.Parse(ShiftBStart), TimeOnly.Parse(ShiftBEnd));

        if (Contains(a, t)) return "TeamA";
        if (Contains(b, t)) return "TeamB";
        return "TeamC";
    }

    private static bool Contains((TimeOnly start, TimeOnly end) w, TimeOnly t) =>
        w.start <= w.end ? (t >= w.start && t < w.end) : (t >= w.start || t < w.end);
}