using Application.Dto;
using Application.Interfaces;
using Domain.Abstractions;
using Domain.Models;
using Domain.Ports;
using Domain.Services;
using Infrastructure.Abstraction;

namespace Application.UseCases;

public sealed class CreateChatUseCase(
    IClock clock,
    ISessionRepository sessions,
    IQueueRepository queues,
    IRefusalDecider decider,
    IOfficeHoursService office,
    IQueuePositionProvider positions,
    IIdempotencyStore? idempotency = null) : IUseCase<CreateChatRequest, CreateChatResponse>
{
    private readonly IClock _clock = clock;
    private readonly ISessionRepository _sessions = sessions;
    private readonly IQueueRepository _queues = queues;
    private readonly IRefusalDecider _decider = decider;
    private readonly IOfficeHoursService _office = office;
    private readonly IQueuePositionProvider _positions = positions;
    private readonly IIdempotencyStore? _idempotency = idempotency;

    public async ValueTask<CreateChatResponse> HandleAsync(CreateChatRequest request, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey) &&
            _idempotency?.TryGetSessionId(request.IdempotencyKey!, out var existingId) == true)
        {
            var existing = await _sessions.GetAsync(existingId, ct);
            if (existing is not null)
            {
                var pos = _positions.Position(existing.Id);
                return new CreateChatResponse(
                    Status: "OK",
                    SessionId: existing.Id,
                    PositionInQueue: pos,
                    RefuseReason: null
                );
            }
        }

        var officeHours = _office.IsOfficeHours(_clock.UtcNow);
        var (accept, reason, toOverflow) = _decider.CanAcceptNewSession(
            baseQueueCount: _queues.CountBase,
            baseQueueMax: _queues.BaseMax,
            officeHours: officeHours,
            overflowQueueCount: _queues.CountOverflow,
            overflowQueueMax: _queues.OverflowMax
        );

        if (!accept)
        {
            return new CreateChatResponse(
                Status: "REFUSED",
                SessionId: null,
                PositionInQueue: null,
                RefuseReason: reason
            );
        }

        var sid = Guid.NewGuid();
        var now = _clock.UtcNow;
        var session = new Session(sid, now, fromOverflow: toOverflow);
        await _sessions.UpsertAsync(session, ct);

        if (toOverflow) _queues.EnqueueOverflow(sid);
        else _queues.EnqueueBase(sid);

        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
            _idempotency?.Remember(request.IdempotencyKey!, sid);

        var position = _queues.EnqueueAndGetPosition(sid, toOverflow);

        return new CreateChatResponse(
            Status: "OK",
            SessionId: sid,
            PositionInQueue: position,
            RefuseReason: null
        );
    }
}