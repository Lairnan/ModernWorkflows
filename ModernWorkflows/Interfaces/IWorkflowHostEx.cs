using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace ModernWorkflows.Interfaces;

/// <summary>
/// An extended interface for the WorkflowCore host.
/// Provides additional capabilities such as awaiting workflow completions, 
/// batch loading definitions from files, and simplified event publishing.
/// </summary>
public interface IWorkflowHostEx : IWorkflowHost
{
    /// <summary>
    /// Starts a new workflow instance and asynchronously blocks until it successfully completes.
    /// Throws an exception if the workflow is terminated instead of completing normally.
    /// </summary>
    /// <param name="workflowId">The ID of the workflow definition to start.</param>
    /// <param name="data">The initial data or context object to pass into the workflow.</param>
    /// <param name="reference">An optional custom reference string to associate with the workflow instance.</param>
    /// <returns>A task containing the completed <see cref="WorkflowInstance"/> state.</returns>
    Task<WorkflowInstance> StartWorkflowAndAwaitAsync(string workflowId, object? data = null, string? reference = null);
    
    /// <summary>
    /// Asynchronously waits for a specific running workflow instance to complete.
    /// Throws an exception if the workflow is terminated.
    /// </summary>
    /// <param name="instanceId">The unique identifier of the active workflow instance.</param>
    /// <returns>A task containing the completed <see cref="WorkflowInstance"/> state.</returns>
    Task<WorkflowInstance> AwaitCompleteWorkflow(string instanceId);
    
    /// <summary>
    /// Asynchronously waits for a collection of running workflow instances to complete.
    /// Throws an exception if any of the specified child workflows are terminated.
    /// </summary>
    /// <param name="instanceIds">An array containing the unique identifiers of the workflow instances to wait for.</param>
    /// <returns>A task containing a collection of the completed <see cref="WorkflowInstance"/> states.</returns>
    Task<IEnumerable<WorkflowInstance>> AwaitCompleteWorkflows(string[] instanceIds);
    
    /// <summary>
    /// Scans the application's "Definitions" directory and loads all JSON and YAML workflow definitions.
    /// </summary>
    /// <param name="loader">The definition loader service used to parse and register the files.</param>
    void LoadDefinitions(IDefinitionLoader loader);
    
    /// <summary>
    /// Publishes an event to the workflow engine using the default "ModernWorkflows.Event" event name.
    /// </summary>
    /// <param name="eventKey">The unique routing key for the event to match waiting workflows.</param>
    /// <param name="eventData">The payload or data object to pass along with the event.</param>
    /// <returns>A task that represents the asynchronous publish operation.</returns>
    Task PublishEvent(string eventKey, object eventData);
    
    /// <summary>
    /// Publishes an event to the workflow engine with a explicitly specified event name.
    /// </summary>
    /// <param name="eventName">The name/type of the event. Falls back to the default if left null or whitespace.</param>
    /// <param name="eventKey">The unique routing key for the event to match waiting workflows.</param>
    /// <param name="eventData">The payload or data object to pass along with the event.</param>
    /// <returns>A task that represents the asynchronous publish operation.</returns>
    Task PublishEvent(string eventName, string eventKey, object eventData);
}