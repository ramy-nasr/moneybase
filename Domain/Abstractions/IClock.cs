namespace Domain.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
    DateTime ToLocal(DateTime utc, string timeZoneId);
}
