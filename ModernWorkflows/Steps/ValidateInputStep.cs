using ModernWorkflows.Models;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace ModernWorkflows.Steps;

public class ValidateInputStep : StepBody
{
    public object? Value { get; set; }
    public InputValue? ValueType { get; set; }
    public string MessageError { get; set; } = "";
    
    public override ExecutionResult Run(IStepExecutionContext context)
    {
        if (Value == null || ValueType == null)
        {
            MessageError = "Value is required";
            return ExecutionResult.Outcome(false);
        }

        var outcomeExpr = ValueType switch
        {
            InputValue.Int => Value is int,
            InputValue.Double => Value is double,
            InputValue.Decimal => Value is decimal,
            InputValue.DateTime => Value is DateTime,
            _ => Value is string
        };

        if (!outcomeExpr)
        {
            MessageError = $"{Value} cannot be converted to {ValueType}";
        }

        return ExecutionResult.Outcome(outcomeExpr);
    }
}