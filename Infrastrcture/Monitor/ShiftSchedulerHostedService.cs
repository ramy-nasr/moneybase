using Domain.Ports;
using Infrastructure.Config;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Infrastructure.Monitor
{
    public sealed class ShiftSchedulerHostedService : BackgroundService, IHostedService
    {
        private readonly IAgentRepository _agents;
        private readonly ITeamRepository _teams;
        private readonly IOptions<SupportConfig> _config;
        private readonly string _localTzId;

        public ShiftSchedulerHostedService(
            IAgentRepository agents,
            ITeamRepository teams,
            IOptions<SupportConfig> config)
        {
            _agents = agents;
            _teams = teams;
            _config = config;
            _localTzId = _config.Value.LocalTimeZoneId;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ApplyShiftAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        private async Task ApplyShiftAsync(CancellationToken ct)
        {
            var cfg = _config.Value;
            var nowLocal = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(_localTzId));
            var t = TimeOnly.FromDateTime(nowLocal);

            var baseTeamName = cfg.Teams.GetActiveBaseTeam(t);
            var teams = await _teams.GetAllAsync(ct);
            foreach (var team in teams)
            {
                var agents = await _agents.GetManyAsync(team.AgentIds, ct);
                var isActive = string.Equals(team.Name, baseTeamName, StringComparison.OrdinalIgnoreCase);

                foreach (var a in agents)
                {
                    if (isActive)
                        a.MarkAssignable();
                    else
                        a.MarkNotAssignable();
                    await _agents.UpdateAsync(a, ct);
                }
            }
        }
    }
}
