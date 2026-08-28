using Microsoft.Extensions.Logging;
using ModernWorkflows.Interfaces;
using WorkflowCore.Models;

namespace ModernWorkflows.Models;

public class WorkflowEventPublisher : IWorkflowEventPublisher
{
    private readonly ILogger<WorkflowEventPublisher> _logger;
    private readonly List<ExecutionPointer> _executionPointersHistory = [];
    private readonly List<string> _waitingEvents = [];
 
    private event Action<ExecutionPointer>? WorkflowEvent;
    private event Action<string> WorkflowWaitEventStart;

    public WorkflowEventPublisher(ILogger<WorkflowEventPublisher> logger)
    {
        _logger = logger;
        WorkflowWaitEventStart += WorkflowWaitEventStartProcess;
    }

    private void WorkflowWaitEventStartProcess(string eventName)
    {
        var pointer = _executionPointersHistory.FirstOrDefault(s => s.EventName.Equals(eventName, StringComparison.OrdinalIgnoreCase));
        if (pointer != null)
        {
            _executionPointersHistory.Remove(pointer);
            _waitingEvents.Remove(eventName);
            WorkflowEvent?.Invoke(pointer);
            return;
        }
        _waitingEvents.Add(eventName);
    }

    private void ForceWorkflowEventProcess(ExecutionPointer pointer)
    {
        var waitingEventName = _waitingEvents.FirstOrDefault(s => s.Equals(pointer.EventName, StringComparison.OrdinalIgnoreCase));
        if (waitingEventName != null)
        {
            _waitingEvents.Remove(pointer.EventName);
            WorkflowEvent?.Invoke(pointer);
            return;
        }
        _executionPointersHistory.Add(pointer);
    }

    public async Task PublishEvent(string eventName, string eventKey, object eventData)
    {
        var pointer = new ExecutionPointer
        {
            EventName = eventName,
            EventKey = eventKey,
            EventData = eventData
        };
        _logger.LogDebug("Publish event: {eventName} - {eventKey} with data: {@eventData}", eventName, eventKey, eventData);
        await Task.Run(() => ForceWorkflowEventProcess(pointer));
    }

    public async Task<object> WaitEvent(string eventName, params string[] eventKeys)
    {
        _logger.LogDebug("Waiting event {eventName}, with eventKeys: {@eventKeys}", eventName, eventKeys);
        var tcs = new TaskCompletionSource<object>();
        void Handler(ExecutionPointer evt)
        {
            var evtKey = evt.EventKey;
            if (!string.IsNullOrWhiteSpace(evtKey) && eventKeys.Any(s => s.Equals(evtKey, StringComparison.OrdinalIgnoreCase)))
            {
                tcs.SetResult(evt.EventData);
                _logger.LogDebug("Event {eventName} - {eventKey} handled", evt.EventName, evt.EventKey);
                this.WorkflowEvent -= Handler;
            }
        }
        this.WorkflowEvent += Handler;
        WorkflowWaitEventStart.Invoke(eventName);
        var eventData = await tcs.Task;
        return eventData;
    }
}