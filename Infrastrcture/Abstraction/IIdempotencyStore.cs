namespace Infrastructure.Abstraction;

public interface IIdempotencyStore
{
    bool TryGetSessionId(string key, out Guid sessionId);
    void Remember(string key, Guid sessionId);
}
