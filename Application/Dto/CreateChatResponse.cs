namespace Application.Dto;

public sealed record CreateChatResponse(
    string Status,
    Guid? SessionId,
    int? PositionInQueue,
    string? RefuseReason
);
