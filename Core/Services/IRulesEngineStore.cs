using RulesEngine.Models;

namespace Core.Services
{
    public interface IRulesEngineStore
    {
        Task<List<Workflow>> GetAllWorkflowsAsync();
        Task<Workflow?> GetWorkflowByName(string workflowName);
        Task<List<Workflow>> GetWorkflowByNames(string[] workflowNames);
    }
}