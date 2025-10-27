namespace Application.Dto;

public sealed record PollResponse(
    string Status,
    int? PositionInQueue,
    AgentSummaryDto? Agent
);
