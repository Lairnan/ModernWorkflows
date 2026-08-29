namespace ModernWorkflows.Interfaces;

/// <summary>
/// Defines a contract for interacting with the WorkflowCore event system.
/// Provides mechanisms to publish events to running workflows or halt execution to wait for specific events.
/// </summary>
public interface IWorkflowEventPublisher
{
    /// <summary>
    /// Asynchronously publishes an event to the workflow engine, which can resume workflows that are waiting for it.
    /// </summary>
    /// <param name="eventName">The generic name or type of the event being published.</param>
    /// <param name="eventKey">The specific identifier used to route the event to the correct waiting workflow instance(s).</param>
    /// <param name="eventData">The payload or data object to pass along with the event.</param>
    /// <returns>A task that represents the asynchronous publish operation.</returns>
    Task PublishEvent(string eventName, string eventKey, object eventData);
    
    /// <summary>
    /// Asynchronously halts execution and waits for a specific event to be published to the workflow engine.
    /// </summary>
    /// <param name="eventName">The name or type of the event to listen for.</param>
    /// <param name="eventKeys">An array of acceptable keys to match against incoming events.</param>
    /// <returns>A task containing the event data payload once a matching event is received.</returns>
    Task<object> WaitEvent(string eventName, string[] eventKeys);
}