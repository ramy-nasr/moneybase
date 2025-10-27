namespace Application.Dto;

public sealed record CreateChatRequest(string? IdempotencyKey);
