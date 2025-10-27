namespace Application.Interfaces;

public interface IUseCase<in TRequest, TResponse>
{
    ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken ct);
}
