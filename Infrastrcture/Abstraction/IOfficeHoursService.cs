namespace Infrastructure.Abstraction;

public interface IOfficeHoursService
{
    bool IsOfficeHours(DateTime utcNow);
}
