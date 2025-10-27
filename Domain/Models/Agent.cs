using Domain.ValueObjects;

namespace Domain.Models;

public sealed class Agent(Guid id, string name, Seniority seniority, decimal efficiencyMultiplier, int baseConcurrency)
{
    public Guid Id { get; } = id;
    public string Name { get; } = name;
    public Seniority Seniority { get; } = seniority;
    public decimal EfficiencyMultiplier { get; } = efficiencyMultiplier;
    public int BaseConcurrency { get; } = baseConcurrency;
    public AgentState State { get; private set; } = AgentState.Assignable;
    public int ActiveAssignments { get; private set; } = 0;

    public int CapacitySlots => (int)Math.Floor(BaseConcurrency * EfficiencyMultiplier);

    public bool CanTakeNewAssignment =>
        State == AgentState.Assignable && ActiveAssignments < CapacitySlots;

    public void MarkNotAssignable() => State = AgentState.NotAssignable;

    public void MarkAssignable() => State = AgentState.Assignable;

    public void MarkOffShift() => State = AgentState.OffShift;

    public void IncrementLoad()
    {
        if (!CanTakeNewAssignment) throw new InvalidOperationException("Agent at capacity or not assignable.");
        ActiveAssignments++;
    }

    public void DecrementLoad()
    {
        if (ActiveAssignments == 0) return;
        ActiveAssignments--;
    }
}