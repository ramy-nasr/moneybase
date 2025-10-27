using Domain.Models;

namespace Domain.Ports;

public interface ITeamRepository
{
    ValueTask<Team?> GetByNameAsync(string name, CancellationToken ct);
    ValueTask UpsertAsync(Team team, CancellationToken ct);
    ValueTask<IReadOnlyList<Team>> GetAllAsync(CancellationToken ct);
}
