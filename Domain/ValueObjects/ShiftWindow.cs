namespace Domain.ValueObjects;

public sealed class ShiftWindow(TimeOnly start, TimeOnly end)
{
    public TimeOnly Start { get; } = start;
    public TimeOnly End { get; } = end;
}
