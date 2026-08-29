
using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using ModernWorkflows.Extensions;
using ModernWorkflows.Interfaces;
using TestAtmWorkflow.Implements;
using WorkflowCore.Interface;
using WorkflowCore.Models.LifeCycleEvents;

ConcurrentBag<string> lifeCycleEvents = [];

var file = new FileInfo("Definitions/atm.yaml");
var extension = file.Extension[1..];

IServiceCollection services = new ServiceCollection();
services.AddLogging();
services.AddModernWorkflows();
services.AddTransient<IMessagePresenter, ConsolePresenter>();
services.AddTransient<IWaitInputValue, ConsoleWaitInputValue>();

var provider = services.BuildServiceProvider();
var host = provider.GetRequiredService<IWorkflowHost>();
var definitionLoader = provider.GetRequiredService<IDefinitionLoader>();
((IWorkflowHostEx)host).LoadDefinitions(definitionLoader);

var definition = host.Registry.GetDefinition("AtmWorkflow");
host.Start();
var id = await host.StartWorkflow(definition.Id);

host.OnLifeCycleEvent += evt =>
{
    var json = JsonSerializer.Serialize(new
    {
        Type = evt.GetType().Name,
        TypeString = evt.ToString(),
        Event = evt
    }, new JsonSerializerOptions { WriteIndented = true });
    lifeCycleEvents.Add(json);
};
        
Console.WriteLine($"🚀 Старт workflow [{id}] из '{file.Name}' ({extension})\n");

var tcs = new TaskCompletionSource<bool>();

host.OnLifeCycleEvent += evt =>
{
    if (evt is WorkflowCompleted { WorkflowDefinitionId: "AtmWorkflow" }) tcs.SetResult(true);
};

await tcs.Task;

await Task.Delay(100);

host.Stop();


Console.WriteLine();
Console.WriteLine(string.Join(Environment.NewLine, lifeCycleEvents));