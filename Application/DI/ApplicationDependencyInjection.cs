using Application.Dto;
using Application.Interfaces;
using Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace Application.DI;
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddScoped<IUseCase<CreateChatRequest, CreateChatResponse>, CreateChatUseCase>();
        services.AddScoped<IUseCase<Guid, PollResponse>, PollChatUseCase>();

        return services;
    }
}
