namespace Domain.Services;

public interface IRefusalDecider
{
    (bool Accept, string? Reason, bool ToOverflow) CanAcceptNewSession(
        int baseQueueCount, int baseQueueMax,
        bool officeHours,
        int overflowQueueCount, int overflowQueueMax);
}
