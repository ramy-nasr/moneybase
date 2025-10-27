using Domain.ValueObjects;

namespace Domain.Models;

public sealed class Session(Guid id, DateTime createdAtUtc, bool fromOverflow)
{
    public Guid Id { get; } = id;
    public DateTime CreatedAtUtc { get; } = createdAtUtc;
    public SessionStatus Status { get; private set; } = SessionStatus.Queued;
    public Guid? AssignedAgentId { get; private set; } = null;
    public DateTime LastSeenUtc { get; private set; } = createdAtUtc;
    public bool FromOverflow { get; } = fromOverflow;

    public void Touch(DateTime nowUtc) => LastSeenUtc = nowUtc;

    public void Assign(Guid agentId)
    {
        if (Status is SessionStatus.Inactive or SessionStatus.Ended)
            throw new InvalidOperationException("Cannot assign ended or inactive session.");
        AssignedAgentId = agentId;
        Status = SessionStatus.Assigned;
    }

    public void MarkInactive()
    {
        if (Status == SessionStatus.Ended) return;
        Status = SessionStatus.Inactive;
    }
}