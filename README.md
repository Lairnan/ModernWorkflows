# ModernWorkflows
[![Russian](https://img.shields.io/badge/Русский-RU-blue)](./README.ru.md)
[![NuGet version](https://img.shields.io/nuget/v/ModernWorkflows.svg?label=NuGet)](https://www.nuget.org/packages/ModernWorkflows)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/github/license/lairnan/ModernWorkflows)](LICENSE)
---
ModernWorkflows is a thin layer on top of [WorkflowCore](https://github.com/danielgerlag/workflow-core) and its YAML/JSON DSL extension. It adds a set of ready-to-use workflows defined as plain C# classes (no YAML files required), a step for calling one workflow from another as a sub-routine, and a request/response bridge for workflows that need to wait for an external event.

It is meant for applications that mix strongly-typed C# workflows with data-driven YAML/JSON workflows and need both to interoperate.

### Core concepts used in the project:
- `ShowMessage`, `WaitInputValue` (built-in workflows, plain `IWorkflow<TData>` classes, registered automatically);
- `StartWorkflowStep` (a step that starts another registered workflow — C# or YAML — and maps inputs/outputs by expression);
- `IWorkflowHostEx` (extends `IWorkflowHost` with `StartWorkflowAndAwaitAsync`, `AwaitCompleteWorkflow(s)` and `LoadDefinitions` for YAML/JSON);
- `IWorkflowEventPublisher`, `WaitEvent` (publish/await an external event from inside a workflow);
- `IMessagePresenter`, `IWaitInputValue` (the two interfaces you implement to plug your own UI/console/API into `ShowMessage` and `WaitInputValue`).

### Installation
```
Target framework: .NET 8 (works on higher versions as well)
Install via NuGet:
- dotnet add package ModernWorkflows
Install via Package Manager:
- Install-Package ModernWorkflows
```

---

## Usage examples

### Example 1

#### Registering ModernWorkflows in ServiceCollection
```csharp
var serviceCollection = new ServiceCollection();
serviceCollection.AddLogging();
serviceCollection.AddModernWorkflows();
```

#### Implementing the interfaces used by the built-in workflows
```csharp
public class ConsolePresenter : IMessagePresenter
{
    public void Show(string message, bool newLineAfter = false)
        => Console.WriteLine(newLineAfter ? message + Environment.NewLine : message);
}

public class ConsoleWaitInputValue : IWaitInputValue
{
    public string WaitStringInput(string titleKey)
    {
        Console.Write(titleKey);
        return Console.ReadLine() ?? "";
    }

    public int WaitIntInput(string titleKey) { /* ... */ }
    public double WaitDoubleInput(string titleKey) { /* ... */ }
    public decimal WaitDecimalInput(string titleKey, int decimalPoint) { /* ... */ }
    public DateTime WaitDateTimeInput(string titleKey, string dateFormat = "dd.MM.yyyy") { /* ... */ }
}
```

#### Starting the built-in `WaitInputValue` workflow and getting the result
```csharp
var serviceCollection = new ServiceCollection();
serviceCollection.AddLogging();
serviceCollection.AddModernWorkflows();
serviceCollection.AddTransient<IMessagePresenter, ConsolePresenter>();
serviceCollection.AddTransient<IWaitInputValue, ConsoleWaitInputValue>();

var serviceProvider = serviceCollection.BuildServiceProvider();
var host = (IWorkflowHostEx)serviceProvider.GetRequiredService<IWorkflowHost>();
host.Start();

var completed = await host.StartWorkflowAndAwaitAsync("WaitInputValue", new InputValueContext
{
    TitleKey = "Enter your PIN: ",
    InputValueType = InputValue.Int
});

var pin = (int)((InputValueContext)completed.Data).Value!;
Console.WriteLine($"Pin: {pin}");
// Execution result:
// Enter your PIN: (waits for console input, re-asks on invalid values)
// Pin: 1234
```

### Example 2

#### Defining your own workflow as a plain C# class
```csharp
public class MyWorkflowData
{
    public string Name { get; set; }
}

public class GreetStep : StepBody
{
    public string Name { get; set; } = null!;

    public override ExecutionResult Run(IStepExecutionContext context)
    {
        Console.WriteLine($"Hello, {Name}!");
        return ExecutionResult.Next();
    }
}

public class MyWorkflow : IWorkflow<MyWorkflowData>
{
    public string Id => "MyWorkflow";
    public int Version => 1;

    public void Build(IWorkflowBuilder<MyWorkflowData> builder)
    {
        builder
            .StartWith<GreetStep>()
                .Input(step => step.Name, data => data.Name)
            .Then<StartWorkflowStep>()
                .Input(step => step.WorkflowId, data => "ShowMessage")
                .Input(step => step.ChildInputs, data => JObject.Parse("{ \"Message\": \"data.Name\" }"));
    }
}
```

#### Registering and starting it
```csharp
var serviceProvider = serviceCollection.BuildServiceProvider();
var host = serviceProvider.GetRequiredService<IWorkflowHost>();

host.RegisterWorkflow<MyWorkflow, MyWorkflowData>();
host.Start();

await host.StartWorkflow("MyWorkflow", new MyWorkflowData { Name = "Alice" });
// Execution result:
// Hello, Alice!
// Alice
```

`StartWorkflowStep` here calls the built-in `ShowMessage` workflow as a sub-routine,
the same way it can call a workflow loaded from YAML/JSON — `ChildInputs`/`ChildOutputs`
accept either a quoted literal (`'"some literal"'`) or a `data.`/`step.` expression resolved
against the calling workflow's data.

---

## Compatibility
- .NET 8+

## License
The project is licensed under the Apache License.
See the [LICENSE](./LICENSE) file for details.