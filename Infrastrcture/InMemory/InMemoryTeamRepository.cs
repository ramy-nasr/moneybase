using Domain.Models;
using Domain.Ports;
using System.Collections.Concurrent;


namespace Infrastructure.InMemory;

public sealed class InMemoryTeamRepository : ITeamRepository
{
    private readonly ConcurrentDictionary<string, Team> _teams = new(StringComparer.OrdinalIgnoreCase);

    public ValueTask<Team?> GetByNameAsync(string name, CancellationToken ct) =>
        ValueTask.FromResult(_teams.TryGetValue(name, out var t) ? t : null);

    public ValueTask UpsertAsync(Team team, CancellationToken ct)
    {
        _teams[team.Name] = team;
        return ValueTask.CompletedTask;
    }

    public ValueTask<IReadOnlyList<Team>> GetAllAsync(CancellationToken ct) =>
        ValueTask.FromResult((IReadOnlyList<Team>)_teams.Values.ToList());
}
