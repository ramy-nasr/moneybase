using Domain.Models;
using Domain.Ports;
using System.Collections.Concurrent;


namespace Infrastructure.InMemory;

public sealed class InMemoryChatRepository : ISessionRepository
{
    private readonly ConcurrentDictionary<Guid, Session> _sessions = new();

    public ValueTask<Session?> GetAsync(Guid id, CancellationToken ct) =>
        ValueTask.FromResult(_sessions.TryGetValue(id, out var s) ? s : null);

    public ValueTask UpsertAsync(Session session, CancellationToken ct)
    {
        _sessions[session.Id] = session;
        return ValueTask.CompletedTask;
    }

    public ValueTask<IReadOnlyList<Session>> GetAllAsync(CancellationToken ct) =>
        ValueTask.FromResult((IReadOnlyList<Session>)_sessions.Values.ToList());
}
