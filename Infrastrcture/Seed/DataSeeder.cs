using Domain.Models;
using Domain.Ports;
using Domain.Services;
using Domain.ValueObjects;
using Infrastructure.Config;

namespace Infrastructure.Seed;

public sealed class DataSeeder
{
    private readonly IAgentRepository _agents;
    private readonly ITeamRepository _teams;
    private readonly IQueueRepository _queues;
    private readonly ICapacityCalculator _capacity;
    private readonly SupportConfig _cfg;

    public DataSeeder(
        IAgentRepository agents,
        ITeamRepository teams,
        IQueueRepository queues,
        ICapacityCalculator capacity,
        SupportConfig cfg)
    {
        _agents = agents;
        _teams = teams;
        _queues = queues;
        _capacity = capacity;
        _cfg = cfg;
    }

    public async Task SeedAsync(CancellationToken ct)
    {
        var anyTeams = (await _teams.GetAllAsync(ct)).Any();
        if (anyTeams) { await EnsureQueueLimitsAsync(ct); return; }

        var baseConc = _cfg.BaseConcurrencyPerAgent;

        var teamAIds = new List<Guid>();
        for (int i = 0; i < _cfg.Teams.TeamA_TeamLead; i++)
            teamAIds.Add(await AddAgentAsync($"A_TL_{i}", Seniority.TeamLead, 0.5m, baseConc, ct));
        for (int i = 0; i < _cfg.Teams.TeamA_Mid; i++)
            teamAIds.Add(await AddAgentAsync($"A_MID_{i}", Seniority.Mid, 0.6m, baseConc, ct));
        for (int i = 0; i < _cfg.Teams.TeamA_Junior; i++)
            teamAIds.Add(await AddAgentAsync($"A_JR_{i}", Seniority.Junior, 0.4m, baseConc, ct));
        await _teams.UpsertAsync(new Team("TeamA", teamAIds), ct);

        var teamBIds = new List<Guid>();
        for (int i = 0; i < _cfg.Teams.TeamB_Senior; i++)
            teamBIds.Add(await AddAgentAsync($"B_SR_{i}", Seniority.Senior, 0.8m, baseConc, ct));
        for (int i = 0; i < _cfg.Teams.TeamB_Mid; i++)
            teamBIds.Add(await AddAgentAsync($"B_MID_{i}", Seniority.Mid, 0.6m, baseConc, ct));
        for (int i = 0; i < _cfg.Teams.TeamB_Junior; i++)
            teamBIds.Add(await AddAgentAsync($"B_JR_{i}", Seniority.Junior, 0.4m, baseConc, ct));
        await _teams.UpsertAsync(new Team("TeamB", teamBIds), ct);

        var teamCIds = new List<Guid>();
        for (int i = 0; i < _cfg.Teams.TeamC_Mid; i++)
            teamCIds.Add(await AddAgentAsync($"C_MID_{i}", Seniority.Mid, 0.6m, baseConc, ct));
        await _teams.UpsertAsync(new Team("TeamC", teamCIds), ct);

        await EnsureQueueLimitsAsync(ct);
    }

    private async Task EnsureQueueLimitsAsync(CancellationToken ct)
    {
        var localTz = TimeZoneInfo.FindSystemTimeZoneById(_cfg.LocalTimeZoneId);
        var nowLocal = TimeZoneInfo.ConvertTime(DateTime.UtcNow, localTz);
        var activeTeamName = _cfg.Teams.GetActiveBaseTeam(TimeOnly.FromDateTime(nowLocal));

        var activeTeam = await _teams.GetByNameAsync(activeTeamName, ct)
                         ?? (await _teams.GetAllAsync(ct)).FirstOrDefault();
        if (activeTeam is null) return;

        var activeAgents = await _agents.GetManyAsync(activeTeam.AgentIds, ct);
        var baseTeamCapacity = _capacity.ComputeTeamCapacitySlots(activeAgents);
        _queues.BaseMax = _capacity.ComputeQueueMax(baseTeamCapacity, _cfg.QueueFactor);

        var overflowAgentCapacity = (int)Math.Floor(_cfg.BaseConcurrencyPerAgent * _cfg.Overflow.JuniorEfficiency);
        var overflowTeamCapacity = _cfg.Overflow.JuniorCount * overflowAgentCapacity;
        _queues.OverflowMax = _capacity.ComputeQueueMax(overflowTeamCapacity, _cfg.QueueFactor);
    }

    private async Task<Guid> AddAgentAsync(
        string name, Seniority s, decimal eff, int baseConcurrency, CancellationToken ct)
    {
        var id = Guid.NewGuid();
        var agent = new Agent(id, name, s, eff, baseConcurrency);
        agent.MarkNotAssignable(); 
        await _agents.UpdateAsync(agent, ct);
        return id;
    }
}