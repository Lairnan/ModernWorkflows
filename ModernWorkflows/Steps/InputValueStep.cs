using ModernWorkflows.Interfaces;
using ModernWorkflows.Models;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace ModernWorkflows.Steps;

public class InputValueStep : StepBody
{
    private readonly IWaitInputValue _waitInputValue;
    
    public object OutputValue { get; set; }

    public string TitleKey { get; set; }
    public int? DecimalPoint { get; set; }
    public string? InputFormat { get; set; }

    public InputValue InputValueType { get; set; }

    public InputValueStep(IWaitInputValue waitInputValue)
    {
        _waitInputValue = waitInputValue;
    }
    
    public override ExecutionResult Run(IStepExecutionContext context)
    {
        OutputValue = InputValueType switch
        {
            InputValue.Int => _waitInputValue.WaitIntInput(TitleKey),
            InputValue.Double => _waitInputValue.WaitDoubleInput(TitleKey),
            InputValue.Decimal => _waitInputValue.WaitDecimalInput(TitleKey, DecimalPoint ?? 2),
            InputValue.DateTime => _waitInputValue.WaitDateTimeInput(TitleKey, InputFormat ?? "dd.MM.yyyy"),
            _ => _waitInputValue.WaitStringInput(TitleKey)
        };
        return ExecutionResult.Next();
    }
}