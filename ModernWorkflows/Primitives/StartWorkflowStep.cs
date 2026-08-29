using DynamicExpresso;
using Microsoft.Extensions.Logging;
using ModernWorkflows.Interfaces;
using Newtonsoft.Json.Linq;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace ModernWorkflows.Primitives;

public class StartWorkflowStep(ILogger<StartWorkflowStep> logger, IWorkflowHostEx workflowHost) : StepBodyAsync
{
    public string WorkflowId { get; set; } = null!;
    public JObject? ChildInputs { get; set; }
    public JObject? ChildOutputs { get; set; }
    
    public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        var definition = workflowHost.Registry.GetDefinition(WorkflowId);
        var childDataType = definition.DataType;
        var inputData = ChildInputs != null ? GetInputData(context, childDataType, ChildInputs) : new JObject().ToObject(childDataType);
        
        logger.LogDebug("StartWorkflowStep id: {workflowId} with dataType: {dataType} and data: {@data}", WorkflowId, childDataType.FullName, inputData);

        var completedWorkflow = await workflowHost.StartWorkflowAndAwaitAsync(WorkflowId, inputData);
        
        if (ChildOutputs != null)
            SetChildOutputs(context, completedWorkflow.Data, ChildOutputs);
        
        return ExecutionResult.Next();
    }

    private object? GetInputData(IStepExecutionContext parentContext, Type childDataType, JObject childInputs)
    {
        var resolvedInputs = new JObject();

        foreach (var property in childInputs.Properties())
        {
            var value = ResolveValue(property.Value, parentContext);
            resolvedInputs[property.Name] = JToken.FromObject(value);
        }

        return resolvedInputs.ToObject(childDataType);
    }
    
    private void SetChildOutputs(IStepExecutionContext parentContext, object childData, JObject childOutputs)
    {
        foreach (var mapping in childOutputs.Properties())
        {
            var childValue = GetPropertyValue(childData, mapping.Value.ToString());
            if (childValue != null)
                SetParentPropertyValue(parentContext.Workflow.Data, mapping.Name, childValue);
        }
    }

    private object? ResolveValue(JToken token, IStepExecutionContext context)
    {
        if (token.Type == JTokenType.String)
        {
            var tokenValue = token.Value<string>()!;

            return IsQuotedString(tokenValue)
                ? Unquote(tokenValue)
                : ResolveExpression(tokenValue, context.Workflow.Data, "data");
        }

        return token.ToObject<object>();
    }

    private bool IsQuotedString(string input)
    {
        return input.StartsWith("\"") && input.EndsWith("\"");
    }

    private string Unquote(string input)
    {
        return input.Trim('"');
    }

    private object? ResolveExpression(string expression, object data, string contextDataStart)
    {
        var contextDataStartWithDot = contextDataStart + ".";
        if (expression.StartsWith(contextDataStartWithDot))
        {
            var propertyName = expression.Substring(contextDataStartWithDot.Length);
            var dataProperty = data.GetType().GetProperty(propertyName);
            return dataProperty?.GetValue(data);
        }

        var interpreter = new Interpreter();
        interpreter.SetVariable(contextDataStart, data);
        interpreter.Reference(typeof(DateTime));
        interpreter.Reference(typeof(Guid));
        interpreter.Reference(typeof(Math));

        try
        {
            return interpreter.Eval(expression);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Не удалось вычислить выражение '{expression}': {ex.Message}", ex);
        }
    }
    
    private object? GetPropertyValue(object obj, string propertyName)
    {
        return IsQuotedString(propertyName)
            ? Unquote(propertyName)
            : ResolveExpression(propertyName, obj, "step");
    }

    private void SetParentPropertyValue(object parentData, string propertyName, object value)
    {
        var propertyInfo = parentData.GetType().GetProperty(propertyName);
        if (propertyInfo != null && propertyInfo.CanWrite)
        {
            var convertedValue = Convert.ChangeType(value, propertyInfo.PropertyType);
            propertyInfo.SetValue(parentData, convertedValue);
        }
    }
}