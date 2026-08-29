# ModernWorkflows
[![English](https://img.shields.io/badge/English-EN-blue)](./README.md)
[![NuGet version](https://img.shields.io/nuget/v/ModernWorkflows.svg?label=NuGet)](https://www.nuget.org/packages/ModernWorkflows)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/github/license/lairnan/ModernWorkflows)](LICENSE)
---
ModernWorkflows — это тонкий слой поверх [WorkflowCore](https://github.com/danielgerlag/workflow-core) и его YAML/JSON DSL-расширения. Он добавляет набор готовых к использованию workflow, описанных обычными C#-классами (без зависимости от yaml-файлов), шаг для вызова одного workflow из другого как подпроцесса, а также мост запрос/ответ для workflow, которым нужно дождаться внешнего события.

Библиотека рассчитана на приложения, которые совмещают строго типизированные C#-workflow с данными-workflow на YAML/JSON и которым нужно, чтобы то и другое работало вместе.

### Основные концепции проекта:
- `ShowMessage`, `WaitInputValue` (встроенные workflow, обычные классы `IWorkflow<TData>`, регистрируются автоматически);
- `StartWorkflowStep` (шаг, который запускает другой зарегистрированный workflow — C# или YAML — и пробрасывает входные/выходные данные через выражения);
- `IWorkflowHostEx` (расширяет `IWorkflowHost` методами `StartWorkflowAndAwaitAsync`, `AwaitCompleteWorkflow(s)` и `LoadDefinitions` для YAML/JSON);
- `IWorkflowEventPublisher`, `WaitEvent` (публикация/ожидание внешнего события внутри workflow);
- `IMessagePresenter`, `IWaitInputValue` (два интерфейса, через которые ты подключаешь свой UI/консоль/API к `ShowMessage` и `WaitInputValue`).

### Установка
```
Целевой фреймворк: .NET 8 (работает и на более новых версиях)
Установка через NuGet:
- dotnet add package ModernWorkflows
Установка через Package Manager:
- Install-Package ModernWorkflows
```

---

## Примеры использования

### Пример 1

#### Регистрация ModernWorkflows в ServiceCollection
```csharp
var serviceCollection = new ServiceCollection();
serviceCollection.AddLogging();
serviceCollection.AddModernWorkflows();
```

#### Реализация интерфейсов, которые использует встроенные workflow
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

#### Запуск встроенного workflow `WaitInputValue` и получение результата
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
    TitleKey = "Введите ваш пин-код: ",
    InputValueType = InputValue.Int
});

var pin = (int)((InputValueContext)completed.Data).Value!;
Console.WriteLine($"Пин-код: {pin}");
// Результат выполнения:
// Введите ваш пин-код: (ожидает ввод в консоли, переспрашивает при некорректном значении)
// Пин-код: 1234
```

### Пример 2

#### Описание своего workflow обычным C#-классом
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
        Console.WriteLine($"Привет, {Name}!");
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

#### Регистрация и запуск
```csharp
var serviceProvider = serviceCollection.BuildServiceProvider();
var host = serviceProvider.GetRequiredService<IWorkflowHost>();

host.RegisterWorkflow<MyWorkflow, MyWorkflowData>();
host.Start();

await host.StartWorkflow("MyWorkflow", new MyWorkflowData { Name = "Алиса" });
// Результат выполнения:
// Привет, Алиса!
// Алиса
```

Здесь `StartWorkflowStep` вызывает встроенный workflow `ShowMessage`
как подпроцесс — точно так же он может вызвать workflow, загруженный
из YAML/JSON. `ChildInputs`/`ChildOutputs` принимают либо литерал в
кавычках (`'"строка"'`), либо выражение `data.`/`step.`, вычисляемое
относительно данных вызывающего workflow.

---

## Совместимость
- .NET 8+

## Лицензия
Проект распространяется под лицензией Apache.
Подробности — в файле [LICENSE](./LICENSE).