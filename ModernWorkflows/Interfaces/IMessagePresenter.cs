namespace ModernWorkflows.Interfaces;

public interface IMessagePresenter
{
    void Show(string message, bool newLineAfter = false);
}