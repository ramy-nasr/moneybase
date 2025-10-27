using Domain.Models;
using Domain.Ports;
using System.Collections.Concurrent;


namespace Infrastructure.InMemory;

public sealed class InMemoryAgentRepository : IAgentRepository
{
    private readonly ConcurrentDictionary<Guid, Agent> _agents = new();

    public ValueTask<Agent?> GetAsync(Guid id, CancellationToken ct) =>
        ValueTask.FromResult(_agents.TryGetValue(id, out var a) ? a : null);

    public ValueTask<IReadOnlyList<Agent>> GetManyAsync(IEnumerable<Guid> ids, CancellationToken ct)
    {
        var list = new List<Agent>();
        foreach (var id in ids)
            if (_agents.TryGetValue(id, out var a)) list.Add(a);
        return ValueTask.FromResult((IReadOnlyList<Agent>)list);
    }

    public ValueTask<IReadOnlyList<Agent>> GetAllAsync(CancellationToken ct) =>
        ValueTask.FromResult((IReadOnlyList<Agent>)_agents.Values.ToList());

    public ValueTask UpdateAsync(Agent agent, CancellationToken ct)
    {
        _agents[agent.Id] = agent;
        return ValueTask.CompletedTask;
    }

    public void Upsert(Agent agent) => _agents[agent.Id] = agent;
}
