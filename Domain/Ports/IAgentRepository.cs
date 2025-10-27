using Domain.Models;
namespace Domain.Ports;

public interface IAgentRepository
{
    ValueTask<Agent?> GetAsync(Guid id, CancellationToken ct);
    ValueTask<IReadOnlyList<Agent>> GetManyAsync(IEnumerable<Guid> ids, CancellationToken ct);
    ValueTask<IReadOnlyList<Agent>> GetAllAsync(CancellationToken ct);
    ValueTask UpdateAsync(Agent agent, CancellationToken ct);
}
