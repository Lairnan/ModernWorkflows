using ModernWorkflows.Interfaces;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace ModernWorkflows.Primitives;

public class WaitEvent : StepBodyAsync
{
    private readonly IWorkflowEventPublisher _workflowEventPublisher;
    
    public WaitEvent(IWorkflowEventPublisher workflowEventPublisher)
    {
        _workflowEventPublisher = workflowEventPublisher;
    }
    
    public string? EventKey { get; set; }
    public string[]? EventKeys { get; set; }
    public string? EventName { get; set; }
    public object EventData { get; set; }
    
    
    public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        if (string.IsNullOrWhiteSpace(this.EventKey) && (this.EventKeys == null || this.EventKeys.Length < 1))
            throw new ArgumentException("Event key or keys cannot be empty");
        
        if (string.IsNullOrWhiteSpace(this.EventName))
            this.EventName = "SmartReminder.Event";

        var eventParameters = string.IsNullOrWhiteSpace(this.EventKey) ? this.EventKeys! : [this.EventKey];
        this.EventData = await _workflowEventPublisher.WaitEvent(this.EventName, eventParameters);
        return context.Step.Outcomes.Count > 1 ? ExecutionResult.Outcome(this.EventData) : ExecutionResult.Next();
    }
}