using Domain.Services;

namespace Infrastructure.Policies;

public sealed class RefusalDeciderPolicy : IRefusalDecider
{
    public (bool Accept, string? Reason, bool ToOverflow) CanAcceptNewSession(
        int baseQueueCount, int baseQueueMax,
        bool officeHours,
        int overflowQueueCount, int overflowQueueMax)
    {
        if (baseQueueCount < baseQueueMax)
            return (true, null, false);

        if (!officeHours)
            return (false, "Base queue full and outside office hours; overflow disabled.", false);

        if (overflowQueueCount < overflowQueueMax)
            return (true, null, true);

        return (false, "Both base and overflow queues are full.", true);
    }
}
