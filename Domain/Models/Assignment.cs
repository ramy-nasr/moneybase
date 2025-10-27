namespace Domain.Models;

public sealed class Assignment(Guid sessionId, Guid agentId, DateTime assignedAtUtc)
{
    public Guid SessionId { get; } = sessionId;
    public Guid AgentId { get; } = agentId;
    public DateTime AssignedAtUtc { get; } = assignedAtUtc;
}
