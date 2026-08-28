using ModernWorkflows.Interfaces;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace ModernWorkflows.Steps;

public class ShowMessageStep : StepBody
{
    private readonly IMessagePresenter _presenter;

    public string Message { get; set; } = null!;
    public bool? NewLine { get; set; }

    public ShowMessageStep(IMessagePresenter presenter)
    {
        _presenter = presenter;
    }

    public override ExecutionResult Run(IStepExecutionContext context)
    {
        _presenter.Show(Message, NewLine ?? true);
        return ExecutionResult.Next();
    }
}