namespace Application.Dto;

public sealed record AgentSummaryDto(
    Guid Id,
    string Name,
    string Seniority);
