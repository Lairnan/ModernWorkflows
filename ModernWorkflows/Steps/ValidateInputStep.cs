using ModernWorkflows.Models;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace ModernWorkflows.Steps;

public class ValidateInputStep : StepBody
{
    public object Value { get; set; }
    public InputValue ValueType { get; set; }
    public string MessageError { get; set; } = "";
    
    public override ExecutionResult Run(IStepExecutionContext context)
    {
        if (Value == null || ValueType == null) return ExecutionResult.Outcome(false);
        
        bool outcomeExpr;
        
        switch (ValueType)
        {
            case InputValue.Int:
                outcomeExpr = Value is int;
                break;
            case InputValue.Double:
                outcomeExpr = Value is double;
                break;
            case InputValue.Decimal:
                outcomeExpr = Value is decimal;
                break;
            case InputValue.DateTime:
                outcomeExpr = Value is DateTime;
                break;
            case InputValue.String:
            default:
                outcomeExpr = Value is string;
                break;
        }

        if (!outcomeExpr)
        {
            MessageError = $"{Value} cannot be converted to {ValueType}";
        }

        return ExecutionResult.Outcome(outcomeExpr);
    }
}