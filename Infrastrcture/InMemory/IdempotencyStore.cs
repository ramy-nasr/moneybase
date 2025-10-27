using Infrastructure.Abstraction;
using System.Collections.Concurrent;


namespace Infrastructure.InMemory;

public sealed class IdempotencyStore : IIdempotencyStore
{
    private readonly ConcurrentDictionary<string, Guid> _map = new(StringComparer.Ordinal);

    public bool TryGetSessionId(string key, out Guid id) => _map.TryGetValue(key, out id);

    public void Remember(string key, Guid sessionId) => _map.TryAdd(key, sessionId);
}
