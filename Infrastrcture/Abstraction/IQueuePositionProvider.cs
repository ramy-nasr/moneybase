namespace Infrastructure.Abstraction;

public interface IQueuePositionProvider
{
    int Position(Guid sessionId);
}
