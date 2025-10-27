using Domain.Models;

namespace Domain.Services;

public interface IAssignmentPolicy
{
    Agent? NextAssignableAgent(IEnumerable<Agent> assignableAgents);
}
