using Domain.Ports;
using Infrastructure.Abstraction;
using System.Collections.Concurrent;


namespace Infrastructure.InMemory;
public sealed class InMemoryQueueRepository : IQueueRepository, IQueuePositionProvider
{
    private readonly ConcurrentQueue<Guid> _base = new();
    private readonly ConcurrentQueue<Guid> _overflow = new();

    private readonly ConcurrentDictionary<Guid, int> _indexBase = new();
    private readonly ConcurrentDictionary<Guid, int> _indexOverflow = new();

    private int _baseCounter = 0;
    private int _overflowCounter = 0;

    private readonly object _lock = new();

    public int CountBase => _indexBase.Count;
    public int CountOverflow => _indexOverflow.Count;

    public int BaseMax { get; set; }
    public int OverflowMax { get; set; }

    public void EnqueueBase(Guid sessionId)
    {
        lock (_lock)
        {
            var pos = Interlocked.Increment(ref _baseCounter);
            _indexBase[sessionId] = pos;
            _base.Enqueue(sessionId);
        }
    }

    public void EnqueueOverflow(Guid sessionId)
    {
        lock (_lock)
        {
            var pos = Interlocked.Increment(ref _overflowCounter);
            _indexOverflow[sessionId] = pos;
            _overflow.Enqueue(sessionId);
        }
    }

    public int EnqueueAndGetPosition(Guid sessionId, bool toOverflow)
    {
        lock (_lock)
        {
            if (!toOverflow)
            {
                var seq = Interlocked.Increment(ref _baseCounter);
                _indexBase[sessionId] = seq;
                _base.Enqueue(sessionId);

                var minBase = _indexBase.Count == 0 ? seq : _indexBase.Values.Min();
                return (seq - minBase) + 1;
            }
            else
            {
                var seqOv = Interlocked.Increment(ref _overflowCounter);
                _indexOverflow[sessionId] = seqOv;
                _overflow.Enqueue(sessionId);

                var baseAhead = _indexBase.Count;
                var minOv = _indexOverflow.Count == 0 ? seqOv : _indexOverflow.Values.Min();
                return baseAhead + (seqOv - minOv) + 1;
            }
        }
    }

    public bool TryDequeue(out Guid sessionId)
    {
        lock (_lock)
        {
            if (_base.TryDequeue(out sessionId))
            {
                _indexBase.TryRemove(sessionId, out _);
                return true;
            }
            if (_overflow.TryDequeue(out sessionId))
            {
                _indexOverflow.TryRemove(sessionId, out _);
                return true;
            }
            sessionId = Guid.Empty;
            return false;
        }
    }

    public bool Remove(Guid sessionId)
    {
        lock (_lock)
        {
            var removed = _indexBase.TryRemove(sessionId, out _) ||
                          _indexOverflow.TryRemove(sessionId, out _);

            return removed;
        }
    }

    public int Position(Guid sessionId)
    {
        lock (_lock)
        {
            if (_indexBase.TryGetValue(sessionId, out var idx))
            {
                var minBase = _indexBase.Values.DefaultIfEmpty(idx).Min();
                return (idx - minBase) + 1;
            }
            if (_indexOverflow.TryGetValue(sessionId, out var idxOv))
            {
                var baseAhead = _indexBase.Count;
                var minOv = _indexOverflow.Values.DefaultIfEmpty(idxOv).Min();
                return baseAhead + (idxOv - minOv) + 1;
            }
            return -1; 
        }
    }

    int IQueuePositionProvider.Position(Guid sessionId) => Position(sessionId);
}
