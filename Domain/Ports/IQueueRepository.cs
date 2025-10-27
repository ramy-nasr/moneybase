namespace Domain.Ports;

public interface IQueueRepository
{
    int CountBase { get; }
    int CountOverflow { get; }

    void EnqueueBase(Guid sessionId);
    void EnqueueOverflow(Guid sessionId);
    bool TryDequeue(out Guid sessionId);
    bool Remove(Guid sessionId);
    int EnqueueAndGetPosition(Guid sessionId, bool toOverflow);

    int BaseMax { get; set; }
    int OverflowMax { get; set; }
}
