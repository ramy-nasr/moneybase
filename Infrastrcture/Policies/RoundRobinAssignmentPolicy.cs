using Domain.Models;
using Domain.Services;
using Domain.ValueObjects;
using System.Collections.Concurrent;

namespace Infrastructure.Policies;
public sealed class RoundRobinAssignmentPolicy : IAssignmentPolicy
{
    private readonly ConcurrentQueue<Guid> _juniorRing = new();
    private readonly ConcurrentQueue<Guid> _midRing = new();
    private readonly ConcurrentQueue<Guid> _seniorRing = new();
    private readonly ConcurrentQueue<Guid> _leadRing = new();

    private readonly ConcurrentDictionary<Guid, Seniority> _registry = new();

    public void Reset(IEnumerable<Agent> agents)
    {
        _juniorRing.ClearQueue();
        _midRing.ClearQueue();
        _seniorRing.ClearQueue();
        _leadRing.ClearQueue();
        _registry.Clear();

        foreach (var a in agents)
        {
            _registry[a.Id] = a.Seniority;
            switch (a.Seniority)
            {
                case Seniority.Junior: _juniorRing.Enqueue(a.Id); break;
                case Seniority.Mid: _midRing.Enqueue(a.Id); break;
                case Seniority.Senior: _seniorRing.Enqueue(a.Id); break;
                case Seniority.TeamLead: _leadRing.Enqueue(a.Id); break;
            }
        }
    }

    public Agent? NextAssignableAgent(IEnumerable<Agent> assignableAgents)
    {
        var byId = assignableAgents.ToDictionary(a => a.Id, a => a);

        if (TryPopEligible(_juniorRing, byId, out var agent)) return agent;
        if (TryPopEligible(_midRing, byId, out agent)) return agent;
        if (TryPopEligible(_seniorRing, byId, out agent)) return agent;
        if (TryPopEligible(_leadRing, byId, out agent)) return agent;

        return null;
    }

    private static bool TryPopEligible(ConcurrentQueue<Guid> ring, IDictionary<Guid, Agent> map, out Agent? agent)
    {
        int n = ring.Count;
        for (int i = 0; i < n; i++)
        {
            if (!ring.TryDequeue(out var id))
                break;

            if (map.TryGetValue(id, out var candidate) && candidate.CanTakeNewAssignment)
            {
                ring.Enqueue(id);
                agent = candidate;
                return true;
            }

            ring.Enqueue(id);
        }
        agent = null;
        return false;
    }
}

file static class ConcurrentQueueExtensions
{
    public static void ClearQueue<T>(this ConcurrentQueue<T> q)
    {
        while (q.TryDequeue(out _)) { }
    }
}
