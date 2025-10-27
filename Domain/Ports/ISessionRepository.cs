using Domain.Models;

namespace Domain.Ports;


public interface ISessionRepository
{
    ValueTask<Session?> GetAsync(Guid id, CancellationToken ct);
    ValueTask UpsertAsync(Session session, CancellationToken ct);
    ValueTask<IReadOnlyList<Session>> GetAllAsync(CancellationToken ct);
}
