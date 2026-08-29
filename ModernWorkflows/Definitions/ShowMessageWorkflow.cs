using ModernWorkflows.Contexts;
using ModernWorkflows.Steps;
using WorkflowCore.Interface;

namespace ModernWorkflows.Definitions;

public class ShowMessageWorkflow : IWorkflow<ShowMessageContext>
{
    internal const string WorkflowId = "ShowMessage";
    internal const int WorkflowVersion = 1;

    public string Id => WorkflowId;
    public int Version => WorkflowVersion;
    
    public void Build(IWorkflowBuilder<ShowMessageContext> builder)
    {
        builder.StartWith<ShowMessageStep>()
            .Input(step => step.Message, data => data.Message);
    }
}