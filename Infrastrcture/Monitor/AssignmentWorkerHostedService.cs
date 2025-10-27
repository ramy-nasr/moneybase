using Domain.Ports;
using Domain.Services;
using Infrastructure.Policies;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Monitor
{
    public sealed class AssignmentWorkerHostedService(
        IQueueRepository queues,
        IAgentRepository agents,
        ISessionRepository sessions,
        IAssignmentPolicy policy) : BackgroundService, IHostedService
    {
        private readonly IQueueRepository _queues = queues;
        private readonly IAgentRepository _agents = agents;
        private readonly ISessionRepository _sessions = sessions;
        private readonly IAssignmentPolicy _policy = policy;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await AssignLoopAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);
            }
        }

        private async Task AssignLoopAsync(CancellationToken ct)
        {
            var allAgents = await _agents.GetAllAsync(ct);
            var assignable = allAgents.Where(a => a.CanTakeNewAssignment).ToArray();

            if (_policy is RoundRobinAssignmentPolicy rr)
                rr.Reset(assignable);

            var freeSlots = assignable.Sum(a => a.CapacitySlots - a.ActiveAssignments);
            var assignedCount = 0;

            for (int i = 0; i < freeSlots; i++)
            {
                if (!_queues.TryDequeue(out var sid))
                    break;

                var session = await _sessions.GetAsync(sid, ct);
                if (session is null || session.Status is not Domain.ValueObjects.SessionStatus.Queued)
                    continue;

                var nextAgent = _policy.NextAssignableAgent(assignable);
                if (nextAgent is null)
                {
                    if (!session.FromOverflow) _queues.EnqueueBase(sid); else _queues.EnqueueOverflow(sid);
                    break;
                }

                nextAgent.IncrementLoad();
                await _agents.UpdateAsync(nextAgent, ct);

                session.Assign(nextAgent.Id);
                await _sessions.UpsertAsync(session, ct);

                assignedCount++;
            }
        }
    }
}
