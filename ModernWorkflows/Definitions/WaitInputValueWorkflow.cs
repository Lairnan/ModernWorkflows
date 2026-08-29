using ModernWorkflows.Contexts;
using ModernWorkflows.Steps;
using WorkflowCore.Interface;

namespace ModernWorkflows.Definitions;

public class WaitInputValueWorkflow : IWorkflow<InputValueContext>
{
    internal const string WorkflowId = "WaitInputValue";
    internal const int WorkflowVersion = 1;

    public string Id => WorkflowId;
    public int Version => WorkflowVersion;
    
    public void Build(IWorkflowBuilder<InputValueContext> builder)
    {
        builder
            .StartWith<InputValueStep>()
                .Input(step => step.TitleKey, data => data.TitleKey)
                .Input(step => step.DecimalPoint, data => data.DecimalPoint)
                .Input(step => step.InputFormat, data => data.InputFormat)
                .Input(step => step.InputValueType, data => data.InputValueType)
                .Output(data => data.Value, step => step.OutputValue)
            .Then<ValidateInputStep>()
                .Input(step => step.Value, data => data.Value)
                .Input(step => step.ValueType, data => data.InputValueType)
                .Output(data => data.MessageError, step => step.MessageError)
            .While(data => !string.IsNullOrEmpty(data.MessageError))
                .Do(x => x
                    .StartWith<ShowMessageStep>()
                        .Input(step => step.Message, data => data.MessageError)
                    .Then<InputValueStep>()
                        .Input(step => step.TitleKey, data => data.TitleKey)
                        .Input(step => step.DecimalPoint, data => data.DecimalPoint)
                        .Input(step => step.InputFormat, data => data.InputFormat)
                        .Input(step => step.InputValueType, data => data.InputValueType)
                        .Output(data => data.Value, step => step.OutputValue)
                    .Then<ValidateInputStep>()
                        .Input(step => step.Value, data => data.Value)
                        .Input(step => step.ValueType, data => data.InputValueType)
                        .Output(data => data.MessageError, step => step.MessageError)
                );

    }
}