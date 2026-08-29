using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace TestAtmWorkflow.Steps;

public class ValidatePinStep : StepBody
{
    public string? Pin { get; set; }
    public bool IsValid { get; set; }

    public override ExecutionResult Run(IStepExecutionContext context)
    {
        IsValid = Pin == "1234";
        Console.WriteLine(IsValid ? "✅ PIN верный" : "❌ PIN неверный");
        return ExecutionResult.Next();
    }
}