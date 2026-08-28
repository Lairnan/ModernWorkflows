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
    
    public async Task<WorkflowInstance> AwaitCompleteWorkflow(string instanceId)
    {
        var runnableInstances = await this.PersistenceStore.GetRunnableInstances(DateTime.Now);
        if (runnableInstances == null || runnableInstances.All(s => s != instanceId)) return await this.PersistenceStore.GetWorkflowInstance(instanceId);
        
        var tcs = new TaskCompletionSource<bool>();
        void Handler(LifeCycleEvent evt)
        {
            if (evt is WorkflowCompleted wc && wc.WorkflowInstanceId == instanceId)
            {
                tcs.SetResult(true);
                this.OnLifeCycleEvent -= Handler;
            }
            else if (evt is WorkflowTerminated wt && wt.WorkflowInstanceId == instanceId)
            {
                tcs.SetException(new Exception("Child workflow terminated"));
                this.OnLifeCycleEvent -= Handler;
            }
        }
        this.OnLifeCycleEvent += Handler;
        await tcs.Task;
        var completedWorkflow = await this.PersistenceStore.GetWorkflowInstance(instanceId);
        return completedWorkflow;
    }

    public async Task<IEnumerable<WorkflowInstance>> AwaitCompleteWorkflows(string[] instanceIds)
    {
        var instanceIdsCompleted = new HashSet<string>();
        var instanceIdsTerminated = new HashSet<string>();
        var instanceIdsRunning = new HashSet<string>(instanceIds);
        var runnableInstances = await this.PersistenceStore.GetRunnableInstances(DateTime.Now);
        if (runnableInstances == null || runnableInstances.All(s => !instanceIds.Contains(s)))
            return await this.PersistenceStore.GetWorkflowInstances(instanceIds);
        
        var tcs = new TaskCompletionSource<bool>();
        void Handler(LifeCycleEvent evt)
        {
            if (evt is WorkflowCompleted wc && instanceIds.Contains(wc.WorkflowInstanceId))
            {
                instanceIdsCompleted.Add(wc.WorkflowInstanceId);
                instanceIdsRunning.Remove(wc.WorkflowInstanceId);
            }
            else if (evt is WorkflowTerminated wt && instanceIds.Contains(wt.WorkflowInstanceId))
            {
                instanceIdsTerminated.Add(wt.WorkflowInstanceId);
                instanceIdsRunning.Remove(wt.WorkflowInstanceId);
            }

            if (instanceIdsRunning.Count == 0)
            {
                if (instanceIdsTerminated.Count == 0)
                    tcs.SetResult(true);
                else
                    tcs.SetException(new Exception("Child workflow terminated"));
                this.OnLifeCycleEvent -= Handler;
            }
        }
        this.OnLifeCycleEvent += Handler;
        await tcs.Task;
        var completedWorkflows = await this.PersistenceStore.GetWorkflowInstances(instanceIdsCompleted);
        return completedWorkflows;
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
                primaryWorkflowIds = configuration.GetValue<string[]>("PrimaryWorkflowId") ?? ["StartReminder"];
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
        await workflowEventPublisher.PublishEvent("SmartReminder.Event", eventKey, eventData);
    }

    public async Task PublishEvent(string eventName, string eventKey, object eventData)
    {
        if (string.IsNullOrWhiteSpace(eventName)) eventName = "SmartReminder.Event";
        await workflowEventPublisher.PublishEvent(eventName, eventKey, eventData);
    }

    private void LoadFilesDefinitions(IDefinitionLoader loader, IEnumerable<string> filesPath, Func<string, DefinitionSourceV1> deserializer)
    {
        foreach (var path in filesPath)
        {
            var file = File.ReadAllText(path);
            loader.LoadDefinition(file, deserializer);
        }
    }
}