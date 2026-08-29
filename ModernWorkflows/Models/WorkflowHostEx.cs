using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModernWorkflows.Interfaces;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowCore.Models.DefinitionStorage.v1;
using WorkflowCore.Models.LifeCycleEvents;
using WorkflowCore.Services;
using WorkflowCore.Services.DefinitionStorage;

namespace ModernWorkflows.Models;

public class WorkflowHostEx(
    IPersistenceProvider persistenceStore,
    IQueueProvider queueProvider,
    WorkflowOptions options,
    ILoggerFactory loggerFactory,
    IServiceProvider serviceProvider,
    IConfiguration configuration,
    IWorkflowRegistry registry,
    IDistributedLockProvider lockProvider,
    IEnumerable<IBackgroundTask> backgroundTasks,
    IWorkflowController workflowController,
    ILifeCycleEventHub lifeCycleEventHub,
    ISearchIndex searchIndex,
    IActivityController activityController,
    IWorkflowEventPublisher workflowEventPublisher)
    : WorkflowHost(persistenceStore, queueProvider, options, loggerFactory,
        serviceProvider, registry, lockProvider, backgroundTasks, workflowController, lifeCycleEventHub,
        searchIndex, activityController),
        IWorkflowHostEx
{
    private string[]? _primaryWorkflowIds;

    public async Task<WorkflowInstance> StartWorkflowAndAwaitAsync(string workflowId, object? data = null,
        string? reference = null)
    {
        var childWorkflowId = await StartWorkflow(workflowId, data, reference);
        return await AwaitCompleteWorkflow(childWorkflowId);
    }
    
    public async Task<WorkflowInstance> AwaitCompleteWorkflow(string instanceId)
    {
        var runnableInstances = await PersistenceStore.GetRunnableInstances(DateTime.Now);
        if (runnableInstances == null || runnableInstances.All(s => s != instanceId))
            return await PersistenceStore.GetWorkflowInstance(instanceId);
        
        var tcs = new TaskCompletionSource<bool>();
        OnLifeCycleEvent += Handler;
        await tcs.Task;
        var completedWorkflow = await PersistenceStore.GetWorkflowInstance(instanceId);
        return completedWorkflow;

        void Handler(LifeCycleEvent evt)
        {
            switch (evt)
            {
                case WorkflowCompleted wc when wc.WorkflowInstanceId == instanceId:
                    tcs.SetResult(true);
                    OnLifeCycleEvent -= Handler;
                    break;
                case WorkflowTerminated wt when wt.WorkflowInstanceId == instanceId:
                    tcs.SetException(new Exception("Child workflow terminated"));
                    OnLifeCycleEvent -= Handler;
                    break;
            }
        }
    }

    public async Task<IEnumerable<WorkflowInstance>> AwaitCompleteWorkflows(string[] instanceIds)
    {
        var instanceIdsCompleted = new HashSet<string>();
        var instanceIdsTerminated = new HashSet<string>();
        var instanceIdsRunning = new HashSet<string>(instanceIds);
        var runnableInstances = await PersistenceStore.GetRunnableInstances(DateTime.Now);
        if (runnableInstances == null || runnableInstances.All(s => !instanceIds.Contains(s)))
            return await PersistenceStore.GetWorkflowInstances(instanceIds);
        
        var tcs = new TaskCompletionSource<bool>();
        OnLifeCycleEvent += Handler;
        await tcs.Task;
        var completedWorkflows = await PersistenceStore.GetWorkflowInstances(instanceIdsCompleted);
        return completedWorkflows;

        void Handler(LifeCycleEvent evt)
        {
            switch (evt)
            {
                case WorkflowCompleted wc when instanceIds.Contains(wc.WorkflowInstanceId):
                    instanceIdsCompleted.Add(wc.WorkflowInstanceId);
                    instanceIdsRunning.Remove(wc.WorkflowInstanceId);
                    break;
                case WorkflowTerminated wt when instanceIds.Contains(wt.WorkflowInstanceId):
                    instanceIdsTerminated.Add(wt.WorkflowInstanceId);
                    instanceIdsRunning.Remove(wt.WorkflowInstanceId);
                    break;
            }

            if (instanceIdsRunning.Count == 0)
            {
                if (instanceIdsTerminated.Count == 0)
                    tcs.SetResult(true);
                else
                    tcs.SetException(new Exception("Child workflow terminated"));
                OnLifeCycleEvent -= Handler;
            }
        }
    }

    public void LoadDefinitions(IDefinitionLoader loader)
    {
        var folderPath = Path.Combine(AppContext.BaseDirectory, "Definitions");
        
        var yamlFiles = Directory.GetFiles(folderPath, "*.yaml");
        var jsonFiles = Directory.GetFiles(folderPath, "*.json");
        
        LoadFilesDefinitions(loader, yamlFiles, Deserializers.Yaml);
        LoadFilesDefinitions(loader, jsonFiles, Deserializers.Json);
    }

    public string[] GetPrimaryWorkflowId()
    {
        if (_primaryWorkflowIds == null)
        {
            string[] primaryWorkflowIds;
            var primaryWorkflowId = configuration.GetValue<string>("PrimaryWorkflowId");
            if (string.IsNullOrWhiteSpace(primaryWorkflowId))
            {
                primaryWorkflowIds = configuration.GetValue<string[]>("PrimaryWorkflowId") ?? ["ModernWorkflows"];
            }
            else
            {
                primaryWorkflowIds = [primaryWorkflowId];
            }
            _primaryWorkflowIds = primaryWorkflowIds;
        }
        return _primaryWorkflowIds;
    }

    public async Task PublishEvent(string eventKey, object eventData)
    {
        await workflowEventPublisher.PublishEvent("ModernWorkflows.Event", eventKey, eventData);
    }

    public async Task PublishEvent(string eventName, string eventKey, object eventData)
    {
        if (string.IsNullOrWhiteSpace(eventName)) eventName = "ModernWorkflows.Event";
        await workflowEventPublisher.PublishEvent(eventName, eventKey, eventData);
    }

    private static void LoadFilesDefinitions(
        IDefinitionLoader loader,
        IEnumerable<string> filesPath,
        Func<string, DefinitionSourceV1> deserializer
    )
    {
        foreach (var path in filesPath)
        {
            var file = File.ReadAllText(path);
            loader.LoadDefinition(file, deserializer);
        }
    }
}