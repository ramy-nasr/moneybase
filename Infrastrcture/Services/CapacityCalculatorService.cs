using Domain.Models;
using Domain.Services;

namespace Infrastructure.Services
{

    public sealed class CapacityCalculatorService : ICapacityCalculator
    {
        public int ComputeTeamCapacitySlots(IEnumerable<Agent> agents) =>
            agents.Sum(a => a.CapacitySlots);

        public int ComputeQueueMax(int teamCapacitySlots, decimal queueFactor) =>
            (int)Math.Floor(teamCapacitySlots * queueFactor);
    }
}
