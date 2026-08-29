using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace TestAtmWorkflow.Steps;

public class WithdrawCashStep : StepBody
{
    public override ExecutionResult Run(IStepExecutionContext context)
    {
        Console.WriteLine("💵 Выдача наличных: 5000 руб.");
        return ExecutionResult.Next();
    }
}