using Domain.Abstractions;
using Domain.Ports;
using Domain.Services;
using Infrastructure.Abstraction;
using Infrastructure.Config;
using Infrastructure.InMemory;
using Infrastructure.Monitor;
using Infrastructure.Policies;
using Infrastructure.Seed;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastrctureDI(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<SupportConfig>()
                .Bind(config.GetSection("Support"))
                .ValidateOnStart();

        services.AddSingleton(sp => sp.GetRequiredService<
            Microsoft.Extensions.Options.IOptions<SupportConfig>>().Value);
       
        services.AddSingleton<IClock>(sp =>
        {
            var cfg = sp.GetRequiredService<SupportConfig>();
            return new SystemClockService();
        });
        services.AddSingleton<IOfficeHoursService, OfficeHoursService>();

        services.AddSingleton<InMemoryQueueRepository>();
        services.AddSingleton<IQueueRepository>(sp => sp.GetRequiredService<InMemoryQueueRepository>());
        services.AddSingleton<IQueuePositionProvider>(sp => sp.GetRequiredService<InMemoryQueueRepository>());

        services.AddSingleton<IAgentRepository, InMemoryAgentRepository>();
        services.AddSingleton<ISessionRepository, InMemoryChatRepository>();     
        services.AddSingleton<ITeamRepository, InMemoryTeamRepository>();
        services.AddSingleton<IIdempotencyStore, IdempotencyStore>();

        services.AddSingleton<ICapacityCalculator, CapacityCalculatorService>();
        services.AddSingleton<IAssignmentPolicy, RoundRobinAssignmentPolicy>();
        services.AddSingleton<IRefusalDecider, RefusalDeciderPolicy>();

        services.AddSingleton<DataSeeder>();
        services.AddHostedService<SeedDataHostedService>();
        services.AddHostedService<ShiftSchedulerHostedService>();
        services.AddHostedService<InactivityMonitorHostedService>();
        services.AddHostedService<AssignmentWorkerHostedService>();

        return services;
    }
}
