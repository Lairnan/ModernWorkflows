using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace ModernWorkflows.Interfaces;

public interface IWorkflowHostEx : IWorkflowHost
{
    Task<WorkflowInstance> StartWorkflowAndAwaitAsync(string workflowId, object? data = null, string? reference = null);
    Task<WorkflowInstance> AwaitCompleteWorkflow(string instanceId);
    Task<IEnumerable<WorkflowInstance>> AwaitCompleteWorkflows(string[] instanceIds);
    void LoadDefinitions(IDefinitionLoader loader);
    string[] GetPrimaryWorkflowId();
    Task PublishEvent(string eventKey, object eventData);
    Task PublishEvent(string eventName, string eventKey, object eventData);
}