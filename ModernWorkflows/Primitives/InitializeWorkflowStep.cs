using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace ModernWorkflows.Primitives;

public class InitializeWorkflowStep : StepBody
{
    public string WorkflowId { get; set; } = null!;

    public override ExecutionResult Run(IStepExecutionContext context)
    {
        WorkflowId = context.Workflow.Id;
        return ExecutionResult.Next();
    }
}