namespace Domain.Models;

public sealed class Team(string name, IEnumerable<Guid> agentIds)
{
    public string Name { get; } = name;
    public IReadOnlyList<Guid> AgentIds { get; } = [.. agentIds];
}
