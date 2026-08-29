using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace TestAtmWorkflow.Steps;

public class InsertCardStep : StepBody
{
    public override ExecutionResult Run(IStepExecutionContext context)
    {
        Console.Write("Вставьте карту (Enter)...");
        return ExecutionResult.Next();
    }
}