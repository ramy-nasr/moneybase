using Domain.Abstractions;
using Domain.Ports;
using Domain.ValueObjects;
using Infrastructure.Config;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Infrastructure.Monitor
{
    public sealed class InactivityMonitorHostedService(
        IClock clock,
        ISessionRepository sessions,
        IAgentRepository agents,
        IQueueRepository queues,
        IOptions<SupportConfig> config) : BackgroundService, IHostedService
    {
        private readonly IClock _clock = clock;
        private readonly ISessionRepository _sessions = sessions;
        private readonly IAgentRepository _agents = agents;
        private readonly IQueueRepository _queues = queues;
        private readonly IOptions<SupportConfig> _config = config;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await SweepAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromMilliseconds(750), stoppingToken);
            }
        }

        private async Task SweepAsync(CancellationToken ct)
        {
            var now = _clock.UtcNow;
            var timeout = TimeSpan.FromSeconds(_config.Value.InactivitySeconds);

            var all = await _sessions.GetAllAsync(ct);
            foreach (var s in all)
            {
                if (s.Status is SessionStatus.Ended or SessionStatus.Inactive) continue;

                if (now - s.LastSeenUtc >= timeout)
                {
                    if (s.Status == SessionStatus.Queued)
                    {
                        _queues.Remove(s.Id);
                    }
                    else if (s.Status == SessionStatus.Assigned && s.AssignedAgentId is Guid aid)
                    {
                        var agent = await _agents.GetAsync(aid, ct);
                        if (agent is not null)
                        {
                            agent.DecrementLoad();
                            await _agents.UpdateAsync(agent, ct);
                        }
                    }
                    s.MarkInactive();
                    await _sessions.UpsertAsync(s, ct);
                }
            }
        }
    }
}
