using Application.Dto;
using Application.Interfaces;
using Domain.Abstractions;
using Domain.Ports;
using Domain.ValueObjects;
using Infrastructure.Abstraction;

namespace Application.UseCases;

public sealed class PollChatUseCase(
    IClock clock,
    ISessionRepository sessions,
    IAgentRepository agents,
    IQueuePositionProvider positions) : IUseCase<Guid, PollResponse>
{
    private readonly IClock _clock = clock;
    private readonly ISessionRepository _sessions = sessions;
    private readonly IAgentRepository _agents = agents;
    private readonly IQueuePositionProvider _positions = positions;

    public async ValueTask<PollResponse> HandleAsync(Guid sessionId, CancellationToken ct)
    {
        var session = await _sessions.GetAsync(sessionId, ct);
        if (session is null)
            return new PollResponse(Status: "ENDED", PositionInQueue: null, Agent: null);

        if (session.Status is SessionStatus.Ended or SessionStatus.Inactive)
            return new PollResponse(Status: session.Status.ToString().ToUpperInvariant(), PositionInQueue: null, Agent: null);

        session.Touch(_clock.UtcNow);
        await _sessions.UpsertAsync(session, ct);

        if (session.Status == SessionStatus.Queued)
        {
            var pos = _positions.Position(sessionId);
            return new PollResponse(Status: "QUEUED", PositionInQueue: pos, Agent: null);
        }

        if (session.Status == SessionStatus.Assigned && session.AssignedAgentId is Guid aid)
        {
            var agent = await _agents.GetAsync(aid, ct);
            if (agent is null)
                return new PollResponse(Status: "ASSIGNED", PositionInQueue: null, Agent: null);

            var dto = new AgentSummaryDto(agent.Id, agent.Name, agent.Seniority.ToString());
            return new PollResponse(Status: "ASSIGNED", PositionInQueue: null, Agent: dto);
        }

        return new PollResponse(Status: "QUEUED", PositionInQueue: _positions.Position(sessionId), Agent: null);
    }
}