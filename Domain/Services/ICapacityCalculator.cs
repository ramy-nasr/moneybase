using Domain.Models;

namespace Domain.Services;

public interface ICapacityCalculator
{
    int ComputeTeamCapacitySlots(IEnumerable<Agent> agents);
    int ComputeQueueMax(int teamCapacitySlots, decimal queueFactor);
}
