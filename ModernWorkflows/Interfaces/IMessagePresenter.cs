namespace ModernWorkflows.Interfaces;

/// <summary>
/// Defines a base contract for presenting or outputting messages. 
/// Consumers must manually implement this interface to route messages to their desired output (e.g., console, UI, or logs).
/// </summary>
public interface IMessagePresenter
{
    /// <summary>
    /// Displays or outputs the specified message.
    /// </summary>
    /// <param name="message">The text string to be presented.</param>
    /// <param name="newLineAfter">If set to <c>true</c>, appends a line break after the message. Defaults to <c>false</c>.</param>
    void Show(string message, bool newLineAfter = false);
}