namespace ModernWorkflows.Interfaces;

public interface IWorkflowEventPublisher
{
    Task PublishEvent(string eventName, string eventKey, object eventData);
    Task<object> WaitEvent(string eventName, string[] eventKeys);
}